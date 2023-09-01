using Neal.Reddit.Application.Constants.Reddit;
using Neal.Reddit.Client.Interfaces;
using RestSharp;
using RestSharp.Authenticators;

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

    public async Task<string> GetSubredditNewAsync(
        string subredditId,
        string after = "",
        string show = "all",
        int limit = 100)
    {
        var request = new RestRequest($"{UrlStrings.SubredditPartialUrl}/{subredditId}{UrlStrings.NewPartialUrl}")
            .AddParameter(ParameterStrings.After, after)
            .AddParameter(ParameterStrings.Show, show)
            .AddParameter(ParameterStrings.Limit, limit);

        var response = await this._client.GetAsync(request);

        return response.Content ?? string.Empty;
    }
}