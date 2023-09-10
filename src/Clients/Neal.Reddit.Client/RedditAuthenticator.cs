using Neal.Reddit.Client.Models;
using Neal.Reddit.Core.Constants;
using Neal.Reddit.Core.Entities.Exceptions;
using RestSharp;
using RestSharp.Authenticators;
using System.Text.Json;

namespace Neal.Reddit.Client;

/// <summary>
/// Represents a <see cref="AuthenticatorBase"/> for authenticating <see cref="RestClient"/> requests.
/// </summary>
public class RedditAuthenticator : AuthenticatorBase
{
    private readonly string clientId;

    private readonly string clientSecret;

    private readonly string deviceId;

    private DateTimeOffset expires;

    public RedditAuthenticator(Credentials credentials) : base(string.Empty)
    {
        if (credentials is null
            || string.IsNullOrWhiteSpace(credentials.ClientId)
            || string.IsNullOrWhiteSpace(credentials.ClientSecret)
            || string.IsNullOrWhiteSpace(credentials.DeviceId))
        {
            throw new ConfigurationException<Credentials>();
        }

        this.clientId = credentials.ClientId;
        this.clientSecret = credentials.ClientSecret;
        this.deviceId = credentials.DeviceId;
    }

    protected override async ValueTask<Parameter> GetAuthenticationParameter(string accessToken)
    {
        if (string.IsNullOrEmpty(this.Token) || this.expires <= DateTimeOffset.Now)
        {
            await this.GetAuthenticationAsync();
        }

        return new HeaderParameter(KnownHeaders.Authorization, this.Token);
    }

    private async Task GetAuthenticationAsync()
    {
        var options = new RestClientOptions(UrlStrings.RedditBaseUrl)
        {
            Authenticator = new HttpBasicAuthenticator(this.clientId, this.clientSecret)
        };
        var client = new RestClient(options);
        var request = new RestRequest(UrlStrings.TokenPartialUrl)
            .AddParameter(HeaderStrings.GrantTypeKey, HeaderStrings.GrantTypeValue)
            .AddParameter(HeaderStrings.DeviceIdKey, this.deviceId);
        var response = await client.ExecutePostAsync(request);

        if (!response.IsSuccessStatusCode
            || response.Content is null)
        {
            throw new Exception($"Unable to authenticate with response {(int)response.StatusCode} {response.StatusDescription}");
        }

        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(response.Content);

        this.Token = $"{tokenResponse!.TokenType} {tokenResponse!.AccessToken}";
        this.expires = tokenResponse.CreatedAt.AddSeconds(tokenResponse.ExpiresIn);
    }
}