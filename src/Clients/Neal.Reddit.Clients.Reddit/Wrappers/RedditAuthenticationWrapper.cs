using Neal.Reddit.Application.Constants.Reddit;
using Neal.Reddit.Core.Entities.Configuration;
using RestSharp;
using RestSharp.Authenticators;

namespace Neal.Reddit.Clients.Reddit.Wrappers;

public static class RedditAuthenticationWrapper
{
    // TODO: Add retry with Polly
    public static async Task<string?> GetClientRefreshTokenAsync(
        RedditCredentials credentials,
        CancellationToken cancellationToken)
    {
        var options = new RestClientOptions(UrlStrings.RedditApiBaseUrl)
        {
            Authenticator = new HttpBasicAuthenticator(credentials.ClientId, credentials.ClientSecret)
        };
        var client = new RestClient(options);
        var request = new RestRequest(UrlStrings.TokenPartialUrl);

        request.AddParameter(Headers.GrantTypeKey, Headers.GrantTypeValue);
        request.AddParameter(Headers.DeviceIdKey, new Guid().ToString()); // TODO: Cache this value or make a secret/appconfig

        var response = await client.PostAsync(request, cancellationToken);

        return response.Content?.ToString();
    }
}