﻿using Neal.Reddit.Application.Constants.Reddit;
using Neal.Reddit.Client.Interfaces;
using Neal.Reddit.Client.Models;
using Neal.Reddit.Core.Entities.Reddit;
using RestSharp;
using RestSharp.Authenticators;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Neal.Reddit.Client;

public class RedditClient : IRedditClient, IDisposable
{
    private readonly RestClient _client;

    public RedditClient(IAuthenticator authenticator)
    {
        var options = new RestClientOptions(UrlStrings.RedditOathBaseUrl)
        {
            Authenticator = authenticator
        };

        this._client = new RestClient(options);
    }

    public async Task<ApiResponse<Listing<Link>>> GetSubredditPostsNewAsync(
        string subredditId,
        string before = "",
        string after = "",
        string show = "all",
        int limit = 100)
    {
        if (string.IsNullOrWhiteSpace(subredditId))
        {
            throw new ArgumentNullException(nameof(subredditId));
        }

        var path = $"{UrlStrings.SubredditPartialUrl}/{subredditId}{UrlStrings.NewPartialUrl}";

        return await GetSubredditDataAsync<Link>(path, before, after, show, limit);
    }

    public void Dispose()
    {
        this._client?.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<ApiResponse<Listing<T>>> GetSubredditDataAsync<T>(
        string path,
        string before = "",
        string after = "",
        string show = "all",
        int limit = 100) where T : Link
    {
        var request = new RestRequest(path)
            .AddParameter(ParameterStrings.Before, before)
            .AddParameter(ParameterStrings.After, after)
            .AddParameter(ParameterStrings.Show, show)
            .AddParameter(ParameterStrings.Limit, limit)
            .AddParameter(ParameterStrings.RawJson, 1);

        var response = await this._client.ExecuteGetAsync(request);
        var listing = string.IsNullOrWhiteSpace(response?.Content)
            ? default
            : JsonSerializer.Deserialize<DataContainer<Listing<T>>>(
                response.Content,
                new JsonSerializerOptions()
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    PropertyNameCaseInsensitive = true,
                });

        var output = new ApiResponse<Listing<T>>()
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