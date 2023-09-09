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

    public RedditClient(
        IAuthenticator authenticator, 
        ILogger<RedditClient> logger)
    {
        var options = new RestClientOptions(UrlStrings.RedditOathBaseUrl)
        {
            Authenticator = authenticator
        };

        _client = new RestClient(options);
        _logger = logger;
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
        Func<Link, Task> newPostHandler,
        CancellationToken cancellationToken)
    {
        var monitor = new PostMonitor(
            this, 
            configuration, 
            newPostHandler, 
            cancellationToken);

        await monitor.StartAsync();
    }

    public void Dispose()
    {
        _client?.Dispose();
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

        var response = await _client.ExecuteGetAsync(request);
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