using Microsoft.Extensions.Logging;
using Neal.Reddit.Application.Constants.Messages;
using Neal.Reddit.Client.Extensions;
using Neal.Reddit.Client.Interfaces;
using Neal.Reddit.Client.Models;
using Neal.Reddit.Core.Constants;
using Neal.Reddit.Core.Entities.Configuration;
using Neal.Reddit.Core.Entities.Exceptions;
using Neal.Reddit.Core.Entities.Reddit;
using Neal.Reddit.Core.Enums;
using RestSharp;
using RestSharp.Authenticators;
using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Neal.Reddit.Client;

public class RedditClient : IRedditClient, IDisposable
{
    #region Fields

    #region Injected

    private readonly ILogger<RedditClient> logger;

    #endregion

    private readonly RestClient restClient;

    private readonly long startEpochSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    private readonly ConcurrentDictionary<string, int> watchedPosts = new();

    private readonly ConcurrentDictionary<string, int> clientRequestCounts = new();

    #endregion

    public RedditClient(
        IAuthenticator authenticator, 
        ILogger<RedditClient> logger)
    {
        var options = new RestClientOptions(UrlStrings.RedditOathBaseUrl)
        {
            Authenticator = authenticator
        };

        this.restClient = new RestClient(options);
        this.logger = logger;

        this.logger.LogInformation(
            "Application start up [epoch {epoch}] [datetime {date}]",
            this.startEpochSeconds,
            DateTimeOffset.FromUnixTimeSeconds(this.startEpochSeconds));
    }

    public async Task GetPostsAsync(
        SubredditConfiguration configuration,
        Func<Link, Task> postHandler,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(configuration.Name)
            || configuration.PerRequestLimit <= 0)
        {
            throw new ConfigurationException<SubredditConfiguration>();
        }

        var postRequest = new RedditPostRequest(configuration, postHandler);

        do
        {
            await this.RequestPostsAsync(postRequest, cancellationToken);
        }
        while (!cancellationToken.IsCancellationRequested
            && postRequest.ShouldMonitor);

        var requestIdentifier = $"{postRequest.Name}-{postRequest.MonitorType}";

        if (this.clientRequestCounts.TryGetValue(requestIdentifier, out var value))
        {
            _ = this.clientRequestCounts.TryRemove(new(requestIdentifier, value));
        }
    }

    private async Task RequestPostsAsync(RedditPostRequest postRequest, CancellationToken cancellationToken)
    {
        var requestIdentifier = $"{postRequest.Name}-{postRequest.MonitorType}";
        var afterPostName = string.Empty;
        var path = $"{UrlStrings.SubredditPartialUrl}/{postRequest.Name}/{postRequest.Sort}{UrlStrings.Json}".ToLower();
        var requests = 0;

        do
        {
            _ = this.clientRequestCounts.AddOrUpdate(
                requestIdentifier,
                ++requests,
                (_, value) => Math.Max(value, ++requests));

            var response = await this.GetSubredditDataAsync(
                path,
                afterPostName,
                postRequest.Show,
                postRequest.PerRequestLimit,
                cancellationToken);
            var posts = response?.Root?.Data?.Children 
                ?? Enumerable.Empty<DataContainer<Link>>();

            afterPostName = response?.Root?.Data?.After
                ?? string.Empty;

            this.logger.LogInformation(
                "Request response received {posts} posts returned for Subreddit {subreddit}", 
                posts.Count(),
                postRequest.Name);

            foreach (var post in posts)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                if (postRequest.MonitorType == MonitorTypes.AfterStartOnly
                    && post.Data?.CreatedUtcEpoch < this.startEpochSeconds)
                {
                    // If sort is new, ignore remaining posts in request and do not paginate
                    if (postRequest.Sort == Sorts.New)
                    {
                        afterPostName = string.Empty;

                        break;
                    }
                }
                else if (post.Data is not null)
                {
                    var shouldUpdate = this.watchedPosts.TryGetValue(post.Data.Name, out var upvotes)
                        ? upvotes < post.Data.Ups
                        : this.watchedPosts.TryAdd(post.Data.Name, post.Data.Ups);

                    if (shouldUpdate)
                    {
                        this.logger.LogInformation(
                            "Post added or updated [{postName}] in [{subreddit}] [old {upvotes}] [new {newUpvotes}] [{posted}]",
                            post.Data.Name,
                            postRequest.Name,
                            upvotes,
                            post.Data.Ups,
                            DateTimeOffset.FromUnixTimeSeconds((long)post.Data.CreatedUtcEpoch));
                        await postRequest.PostHandler(post.Data);
                    }
                }
            }

            // Delay task processing to moderate request rate for API request limits
            var wait = this.GetThreadSleep(response, postRequest.PerRequestLimit, posts.Count());

            this.logger.LogInformation(
                CommonLogMessages.TaskDelay, 
                wait.TotalSeconds, 
                postRequest.Name);

            await Task.Delay(wait, cancellationToken);
        }
        while (!cancellationToken.IsCancellationRequested
            && !string.IsNullOrEmpty(afterPostName));

        // Reset client request count to number of requests required in the last loop
        this.clientRequestCounts[requestIdentifier] = requests;
    }

    public void Dispose()
    {
        this.restClient?.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<ApiResponse> GetSubredditDataAsync(
        string path,
        string? afterPostName,
        string show,
        int limit,
        CancellationToken cancellationToken)
    {
        var request = new RestRequest(path)
            .AddParameter(ParameterStrings.Show, show)
            .AddParameter(ParameterStrings.Limit, limit)
            .AddParameter(ParameterStrings.RawJson, 1)
            .AddParameter(ParameterStrings.After, afterPostName);    

        this.logger.LogInformation(
            "Executing request for {path} [after {afterPostName}]", 
            path,
            afterPostName);

        var response = await this.restClient.ExecuteGetAsync(request, cancellationToken);

        if (response is not null
            && response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            var wait = TimeSpan.FromSeconds(response.ParseRateLimitReset());

            this.logger.LogWarning(CommonLogMessages.TaskDelay, wait.TotalSeconds, HttpStatusCode.TooManyRequests);

            await Task.Delay(wait, cancellationToken);

            return await this.GetSubredditDataAsync(
                path, 
                afterPostName, 
                show, 
                limit, 
                cancellationToken);  
        }         

        if (response is null
            || response.StatusCode < HttpStatusCode.OK
            || response.StatusCode >= HttpStatusCode.BadRequest)
        {
            var exception = new HttpRequestException(
                string.Format(
                    ExceptionMessages.HttpRequestError,
                    response?.StatusCode,
                    response?.StatusDescription));
            this.logger.LogCritical(
                exception,
                CommonLogMessages.HttpRequestError, 
                response?.StatusCode, 
                response?.StatusDescription);

            throw exception;
        }

        var listing = string.IsNullOrWhiteSpace(response?.Content)
            ? default
            : JsonSerializer.Deserialize<DataContainer<Listing>>(
                response.Content,
                new JsonSerializerOptions()
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    PropertyNameCaseInsensitive = true,
                });

        var output = new ApiResponse()
        {
            RateLimitRemaining = response?.ParseRateLimitRemaining() ?? 0,
            RateLimitUsed = response?.ParseRateLimitUsed() ?? 0,
            RateLimitReset = response?.ParseRateLimitReset() ?? 0,
            Root = listing
        };

        return output;
    }

    private TimeSpan GetThreadSleep(ApiResponse? response, int perRequestLimit, int postCount)
    {
        if (response is null)
        {
            return TimeSpan.FromMilliseconds(Defaults.RetryDelayMilliseconds);
        }

        var runningClientRequests = Math.Max(this.clientRequestCounts.Sum(client => client.Value), 1); // prevent divide by 0
        var limitRemaining = Math.Max(response.RateLimitRemaining, 1); // prevent divide by 0
        var limitingDelay = response.RateLimitReset / limitRemaining / runningClientRequests; // spread client requests across rate limit
        var postCountRatio = (perRequestLimit - postCount) / 100; // convert to percentage
        var postCountDelay = postCountRatio * Defaults.NoPostDelaySeconds; // increase delay based on posts returned
        var delay = Math.Max(limitingDelay + postCountDelay, 1.5); // Ensure minimum delay of 1.5 seconds

        return TimeSpan.FromSeconds(delay);
    }
}