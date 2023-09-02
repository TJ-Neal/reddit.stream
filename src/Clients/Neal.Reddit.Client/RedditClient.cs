using Neal.Reddit.Application.Constants.Reddit;
using Neal.Reddit.Client.Interfaces;
using Neal.Reddit.Client.Models;
using RestSharp;
using RestSharp.Authenticators;
using System.Text.Json;

namespace Neal.Reddit.Client;

public class RedditClient : IRedditClient
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

    public async Task<ApiListingResponse?> GetSubredditNewAsync(
        string subredditId,
        string after = "",
        string show = "all",
        int limit = 100)
    {
        if (string.IsNullOrWhiteSpace(subredditId))
        {
            throw new ArgumentNullException(nameof(subredditId));
        }

        var request = new RestRequest($"{UrlStrings.SubredditPartialUrl}/{subredditId}{UrlStrings.NewPartialUrl}")
            .AddParameter(ParameterStrings.After, after)
            .AddParameter(ParameterStrings.Show, show)
            .AddParameter(ParameterStrings.Limit, limit)
            .AddParameter(ParameterStrings.RawJson, 1);

        var response = await this._client.GetAsync(request);
            
        return string.IsNullOrWhiteSpace(response?.Content)
            ? default
            : JsonSerializer.Deserialize<ApiListingResponse>(response.Content);
    }
}