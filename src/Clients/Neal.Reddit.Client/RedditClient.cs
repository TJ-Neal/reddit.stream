using Microsoft.Extensions.Logging;
using Neal.Reddit.Application.Constants.Messages;
using Neal.Reddit.Application.Utilities;
using Neal.Reddit.Client.Extensions;
using Neal.Reddit.Client.Helpers;
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

/// <inheritdoc cref="IRedditClient"/>
public class RedditClient : IRedditClient, IDisposable
{
    #region Fields

    #region Injected

    private readonly ILogger<RedditClient> logger;

    #endregion

    private readonly RestClient restClient;

    private readonly long startEpochSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    private readonly ConcurrentDictionary<string, int> watchedPosts = new();

    private RateLimiter? rateLimiter;

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

    /// <inheritdoc/>
    public async Task GetPostsAsync(
        SubredditConfiguration configuration,
        Func<Link, CancellationToken, Task> postHandler,
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

    private async Task InitializeRateLimiterAsync()
    {
        var path = $"{UrlStrings.SubredditPartialUrl}/all";
        var request = new RestRequest(path, Method.Head);

        try
        {
            var response = await this.restClient.ExecuteAsync(request);
            var rateLimitRemaining = response?.ParseRateLimitRemaining() ?? Defaults.MaxRequestsPerReset;
            var rateLimitReset = response?.ParseRateLimitReset() ?? Defaults.ResetSeconds;

            this.rateLimiter = new(rateLimitRemaining, rateLimitReset);

            this.logger.LogInformation(
                "Rate limiter initiated with {limitRemaining} remaining before {limitReset} reset.",
                rateLimitRemaining,
                rateLimitReset);
        }
        catch
        {
            this.rateLimiter = new(Defaults.MaxRequestsPerReset, Defaults.ResetSeconds);
            this.logger.LogInformation("Rate limiter initiated with default values.");
        }
    }

    private async Task RequestPostsAsync(RedditPostRequest postRequest, CancellationToken cancellationToken)
    {
        var requestIdentifier = $"{postRequest.Name}-{postRequest.MonitorType}";
        var afterPostName = string.Empty;
        var path = $"{UrlStrings.SubredditPartialUrl}/{postRequest.Name}/{postRequest.Sort}{UrlStrings.Json}".ToLower();
        var updates = 0;

        do
        {
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

                        updates++;

                        this.watchedPosts[post.Data.Name] = post.Data.Ups;

                        await postRequest.PostHandler(post.Data, cancellationToken);
                    }
                }
            }
        }
        while (!cancellationToken.IsCancellationRequested
            && !string.IsNullOrEmpty(afterPostName));

        this.logger.LogInformation("{updates} updates received for {subreddit}", updates, postRequest.Name);

        if (updates < Defaults.PerRequestMaxPosts)
        {
            var updateRatio = (Defaults.PerRequestMaxPosts - updates) / 100.0;
            var delay = TimeSpan.FromSeconds(Defaults.LowUpdateDelaySeconds * updateRatio);

            this.logger.LogInformation(
                "Delaying {delay} seconds for low update count of {updates} for {subreddit}",
                delay.TotalSeconds,
                updates,
                postRequest.Name);

            await Task.Delay(delay, cancellationToken);
        }
    }

    public void Dispose()
    {
        this.restClient?.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<ApiResponse?> GetSubredditDataAsync(
        string path,
        string? afterPostName,
        string show,
        int limit,
        CancellationToken cancellationToken)
    {
        if (this.rateLimiter is null)
        {
            await this.InitializeRateLimiterAsync();
        }

        var request = new RestRequest(path)
            .AddParameter(ParameterStrings.Show, show)
            .AddParameter(ParameterStrings.Limit, limit)
            .AddParameter(ParameterStrings.RawJson, 1)
            .AddParameter(ParameterStrings.After, afterPostName);    

        this.logger.LogInformation(
            "Executing request for {path} [after {afterPostName}]", 
            path,
            afterPostName);

        int retries = 0;

        while (retries < RetryUtilities.MaxRetries)
        {
            try
            {
                var response = await this.rateLimiter
                    !.Run(
                        this.restClient.ExecuteGetAsync(request, cancellationToken),
                        cancellationToken);                   

                if (response is not null
                    && response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    var wait = TimeSpan.FromSeconds(response.ParseRateLimitReset());

                    this.logger.LogWarning(CommonLogMessages.TaskDelay, wait.TotalSeconds, HttpStatusCode.TooManyRequests);
                        
                    await Task.Delay(wait, cancellationToken);

                    this.rateLimiter = null; // Force rate limiter reset            

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

                return new ApiResponse()
                {
                    RateLimitRemaining = response?.ParseRateLimitRemaining() ?? 0,
                    RateLimitUsed = response?.ParseRateLimitUsed() ?? 0,
                    RateLimitReset = response?.ParseRateLimitReset() ?? 0,
                    Root = listing
                };
            }
            catch (Exception ex)
            {
                this.logger.LogCritical(CommonLogMessages.HttpRequestException, ex);

                // Throw if exception is not transient
                if (!RetryUtilities.ExceptionIsTransient(ex))
                {
                    throw;
                }
            }

            retries++;
            await Task.Delay(RetryUtilities.GetDelay(retries), cancellationToken);
        }

        return default;
    }
}