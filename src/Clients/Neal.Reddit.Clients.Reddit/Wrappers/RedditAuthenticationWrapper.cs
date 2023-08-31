using Neal.Reddit.Application.Constants.Reddit;
using RestSharp;
using RestSharp.Authenticators;

namespace Neal.Reddit.Clients.Reddit.Wrappers;

public static class RedditAuthenticationWrapper
{
    // TODO: Add retry with Polly
    public static async Task<string?> GetClientRefreshTokenAsync(
        string clientId,
        string clientSecret,
        CancellationToken cancellationToken)
    {
        var options = new RestClientOptions(UrlStrings.RedditApiBaseUrl)
        {
            Authenticator = new HttpBasicAuthenticator(clientId, clientSecret)
        };
        var client = new RestClient(options);
        var request = new RestRequest(UrlStrings.TokenPartialUrl);

        request.AddParameter(Headers.GrantTypeKey, Headers.GrantTypeValue);
        request.AddParameter(Headers.DeviceIdKey, new Guid().ToString()); // TODO: Cache this value or make a secret/appconfig

        var response = await client.PostAsync(request, cancellationToken);

        return response.Content?.ToString();
    }
}