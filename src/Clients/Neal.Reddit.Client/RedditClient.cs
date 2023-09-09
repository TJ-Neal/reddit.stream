using Microsoft.Extensions.Logging;
using Neal.Reddit.Application.Constants.Messages;
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

    private readonly CancellationToken cancellationToken;

    #endregion

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
        var shouldMonitor = configuration.MonitorType 
            is MonitorTypes.AfterStartOnly or MonitorTypes.All;

        do
        {
            await this.RequestPostsAsync(postRequest, cancellationToken);
        }
        while (!cancellationToken.IsCancellationRequested
            && shouldMonitor);
    }

    private async Task RequestPostsAsync(RedditPostRequest postRequest, CancellationToken cancellationToken)
    {        
        int pertinentPostCount = 0;
        var paginationPost = string.Empty;
        var path = $"{UrlStrings.SubredditPartialUrl}/{postRequest.Name}/{postRequest.Sort}{UrlStrings.Json}";

        do
        {
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

            foreach (var post in posts)
            {
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

                    var lastPost = response?.Root?.Data?.Children?.LastOrDefault();

                    paginationPost = lastPost?.Data?.Name ?? string.Empty;

                    var shouldUpdate = watchedPosts.TryGetValue(post.Data.Name, out int upvotes)
                        ? upvotes < post.Data.Ups
                        : watchedPosts.TryAdd(post.Data.Name, post.Data.Ups);

                    if (shouldUpdate)
                    {
                        await postRequest.PostHandler(post.Data);
                    }
                }
            }

            // TODO: Replace with calculated sleep
            Thread.Sleep(1500);
        }
        while (pertinentPostCount == postRequest.PerRequestLimit 
            && !cancellationToken.IsCancellationRequested);
    }

    public void Dispose()
    {
        restClient?.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<ApiResponse> GetSubredditDataAsync(
        string path,
        string before,
        string after,
        string show,
        int limit,
        CancellationToken cancellationToken)
    {
        var request = new RestRequest(path)            
            .AddParameter(ParameterStrings.Show, show)
            .AddParameter(ParameterStrings.Limit, limit)
            .AddParameter(ParameterStrings.RawJson, 1);

        if (!string.IsNullOrWhiteSpace(before))
        {
            request = request.AddParameter(ParameterStrings.Before, before);
        }
        else if (!string.IsNullOrWhiteSpace(after))
        {
            request = request.AddParameter(ParameterStrings.After, after);
        }

        var response = await restClient.ExecuteGetAsync(request, cancellationToken);

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
            RateLimitRemaining = ParseRateLimitRemaining(response),
            RateLimitUsed = ParseRateLimitUsed(response),
            RateLimitReset = ParseRateLimitReset(response),
            Root = listing
        };

        return output;
    }

    private static double ParseRateLimitRemaining(RestResponse? response)
    {
        var rateLimitRemainingHeader = response
            ?.Headers
            ?.Where(x => x.Name == "x-ratelimit-remaining")
            .Select(x => x.Value)
            .FirstOrDefault();

        _ = double.TryParse(rateLimitRemainingHeader?.ToString(), out var limitRemaining);

        return limitRemaining;
    }

    private static int ParseRateLimitUsed(RestResponse? response)
    {
        var rateLimitUsedHeader = response
            ?.Headers
            ?.Where(x => x.Name == "x-ratelimit-used")
            .Select(x => x.Value)
            .FirstOrDefault();

        
        _ = int.TryParse(rateLimitUsedHeader?.ToString(), out var limitUsed);

        return limitUsed;
    }

    private static int ParseRateLimitReset(RestResponse? response)
    {
        var rateLimitResetHeader = response
            ?.Headers
            ?.Where(x => x.Name == "x-ratelimit-reset")
            .Select(x => x.Value)
            .FirstOrDefault();

        _ = int.TryParse(rateLimitResetHeader?.ToString(), out var limitReset);

        return limitReset;
    }
}