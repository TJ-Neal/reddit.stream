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

    #region Static

    private static readonly ConcurrentDictionary<Guid, int> runningClients = new();

    #endregion

    #region Injected

    private readonly ILogger<RedditClient> logger;

    #endregion

    private readonly Guid identifier = new();

    private readonly RestClient restClient;

    private readonly long startEpochSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    private readonly ConcurrentDictionary<string, int> watchedPosts = new();

    private string startingPost = string.Empty;

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
    }

    private async Task RequestPostsAsync(RedditPostRequest postRequest, CancellationToken cancellationToken)
    {        
        string paginationPost;
        var path = $"{UrlStrings.SubredditPartialUrl}/{postRequest.Name}/{postRequest.Sort}{UrlStrings.Json}".ToLower();
        var requests = 0;

        do
        {
            runningClients.AddOrUpdate(
                this.identifier,
                ++requests,
                (_, value) => Math.Max(value, ++requests));
            paginationPost = string.Empty;

            var pertinentPostCount = 0;
            var response = await GetSubredditDataAsync(
                path,
                this.startingPost,
                paginationPost,
                postRequest.Show,
                postRequest.PerRequestLimit,
                cancellationToken);
            var posts = response
                ?.Root
                ?.Data
                ?.Children 
                    ?? Enumerable.Empty<DataContainer<Link>>();

            this.logger.LogInformation(
                "Request response received\n\t{posts} posts returned for Subreddit\n\t{@subreddit}", 
                posts.Count(),
                postRequest);

            foreach (var post in posts)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                if (postRequest.MonitorType == MonitorTypes.AfterStartOnly
                    && post.Data?.CreatedUtcEpoch < this.startEpochSeconds)
                {
                    if (postRequest.Sort == Sorts.New)
                    {
                        this.startingPost = post.Data.Name;

                        break;
                    }
                }
                else if (post.Data is not null)
                {
                    pertinentPostCount++;

                    var shouldUpdate = watchedPosts.TryGetValue(post.Data.Name, out int upvotes)
                        ? upvotes < post.Data.Ups
                        : watchedPosts.TryAdd(post.Data.Name, post.Data.Ups);

                    if (shouldUpdate)
                    {
                        await postRequest.PostHandler(post.Data);
                    }
                }
            }

            var wait = GetThreadSleep(response, postRequest.PerRequestLimit, posts.Count());

            this.logger.LogInformation(
                CommonLogMessages.TaskDelay, 
                wait.TotalSeconds, 
                postRequest.Name);

            await Task.Delay(wait, cancellationToken);

            if (postRequest.Sort == Sorts.New
                && pertinentPostCount < postRequest.PerRequestLimit)
            {
                paginationPost = string.Empty;
            }
        }
        while (!cancellationToken.IsCancellationRequested
            && !string.IsNullOrWhiteSpace(paginationPost));

        // Reset running client request count to number of requests required in the last loop
        runningClients[this.identifier] = requests;
    }

    public void Dispose()
    {
        if (runningClients.TryGetValue(this.identifier, out var value))
        {
            runningClients.TryRemove(new(this.identifier, value));
        }

        restClient?.Dispose();
        GC.SuppressFinalize(this);
    }

    private static TimeSpan GetThreadSleep(ApiResponse? response, int perRequestLimit, int postCount)
    {
        if (response is null)
        {
            return TimeSpan.FromMilliseconds(Defaults.RetryDelayMilliseconds);
        }                
        
        var runningClientRequests = Math.Max(runningClients.Sum(client => client.Value), 1); // prevent divide by 0
        var limitRemaining = Math.Max(response.RateLimitRemaining, 1); // prevent divide by 0
        var limitingDelay = response.RateLimitReset / limitRemaining / runningClientRequests; // spread client requests across rate limit
        var postCountRatio = (perRequestLimit - postCount) / 100; // convert to percentage
        var postCountDelay = postCountRatio * Defaults.NoPostDelaySeconds; // increase delay based on posts returned

        return TimeSpan.FromSeconds(limitingDelay + postCountDelay);
    }

    private async Task<ApiResponse> GetSubredditDataAsync(
        string path,
        GetOrPostParameter paginationParameter,
        string show,
        int limit,
        CancellationToken cancellationToken)
    {
        var request = new RestRequest(path)
            .AddParameter(ParameterStrings.Show, show)
            .AddParameter(ParameterStrings.Limit, limit)
            .AddParameter(ParameterStrings.RawJson, 1)
            .AddParameter(paginationParameter);

        this.logger.LogInformation("Executing request\n\t{@request}\n\t{startingPost}", request.Parameters, this.startingPost);

        var response = await restClient.ExecuteGetAsync(request, cancellationToken);

        if (response is not null
            && response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            var wait = TimeSpan.FromSeconds(response.ParseRateLimitReset());

            this.logger.LogWarning(CommonLogMessages.TaskDelay, wait.TotalSeconds, HttpStatusCode.TooManyRequests);

            await Task.Delay(wait, cancellationToken);

            return await GetSubredditDataAsync(path, before, after, show, limit, cancellationToken);  
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
            logger.LogCritical(
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
}