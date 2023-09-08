using Microsoft.Extensions.Logging;
using Neal.Reddit.Application.Constants.Reddit;
using Neal.Reddit.Client.Interfaces;
using Neal.Reddit.Client.Models;
using Neal.Reddit.Core.Entities.Exceptions;
using Neal.Reddit.Core.Entities.Reddit;
using RestSharp;
using RestSharp.Authenticators;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Neal.Reddit.Client;

public class RedditClient : IRedditClient, IDisposable
{
    private const int LIMIT = 100;

    private readonly RestClient _client;

    private readonly ILogger<RedditClient> _logger;

    public RedditClient(IAuthenticator authenticator, ILogger<RedditClient> logger)
    {
        var options = new RestClientOptions(UrlStrings.RedditOathBaseUrl)
        {
            Authenticator = authenticator
        };

        this._client = new RestClient(options);
        this._logger = logger;
    }

    public async Task<ApiResponse> GetPostsNewAsync(
        SubredditConfiguration configuration,
        string before = "",
        string after = "",
        string show = "all",
        int limit = LIMIT)
    {
        if (string.IsNullOrWhiteSpace(configuration.Name))
        {
            throw new ConfigurationException<SubredditConfiguration>();
        }

        var path = $"{UrlStrings.SubredditPartialUrl}/{configuration}{UrlStrings.NewPartialUrl}";

        return await GetSubredditDataAsync(path, before, after, show, limit);
    }

    // TODO: Add handler parameter
    public async Task MonitorPostsAsync(
        SubredditConfiguration configuration,
        Action<Link> newPostHandler,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(configuration.Name))
        {
            throw new ConfigurationException<SubredditConfiguration>();
        }

        var startEpochSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var startingPost = string.Empty;
        var watchedPosts = new Dictionary<string, int>();

        while (!cancellationToken.IsCancellationRequested)
        {
            int pertinentPostCount = 0;
            var paginationPost = string.Empty;

            do
            {
                var response = await this.GetPostsNewAsync(configuration, startingPost, paginationPost);
                var posts = response?.Root?.Data?.Children ?? Enumerable.Empty<DataContainer<Link>>();

                foreach (var post in posts)
                {
                    if (configuration.AfterStartOnly
                        && post.Data?.CreatedUtcEpoch < startEpochSeconds)
                    {
                        startingPost = post.Data?.Name;
                    }
                    else if (post.Data is not null)
                    {
                        pertinentPostCount++;
                        // TODO: Add to monitor list/check for change
                        newPostHandler(post.Data);
                    }
                }


                var firstPost = response?.Root?.Data?.Children?.FirstOrDefault();
                var lastPost = response?.Root?.Data?.Children?.LastOrDefault();
                var firstEpoch = DateTimeOffset.FromUnixTimeSeconds((int?)firstPost?.Data?.CreatedUtcEpoch ?? 0);
                var lastEpoch = DateTimeOffset.FromUnixTimeSeconds((int?)lastPost?.Data?.CreatedUtcEpoch ?? 0);
                this._logger.LogInformation(
                    "Posts for {subredditId} : {count}\n\tFirst {firstName} {firstEpoch}\n\tLast {lastName} {lastEpoch}",
                    configuration.Name,
                    response?.Root?.Data?.Count,
                    firstPost?.Data?.Name,
                    firstEpoch,
                    lastPost?.Data?.Name,
                    lastEpoch);
                pertinentPostCount = response?.Root?.Data?.Count ?? 0;
                paginationPost = lastPost?.Data?.Name ?? string.Empty;

                // TODO: Call handler

                // TODO: Replace with calculated sleep
                Thread.Sleep(1500);
            }
            while (pertinentPostCount == LIMIT);
        }
    }

    public void Dispose()
    {
        this._client?.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<ApiResponse> GetSubredditDataAsync(
        string path,
        string before = "",
        string after = "",
        string show = "all",
        int limit = LIMIT)
    {
        var request = new RestRequest(path)            
            .AddParameter(ParameterStrings.Show, show)
            .AddParameter(ParameterStrings.Limit, limit)
            .AddParameter(ParameterStrings.RawJson, 1);

        if (!string.IsNullOrWhiteSpace(before))
        {
            request.AddParameter(ParameterStrings.Before, before);
        }
        else if (!string.IsNullOrWhiteSpace(after))
        {
            request.AddParameter(ParameterStrings.After, after);
        }

        var response = await this._client.ExecuteGetAsync(request);
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