using Neal.Reddit.Application.Constants.Reddit;
using Neal.Reddit.Client.Models;
using RestSharp;
using RestSharp.Authenticators;
using System.Text.Json;

namespace Neal.Reddit.Client;

public class RedditAuthenticator : AuthenticatorBase
{
    private readonly string _clientId;

    private readonly string _clientSecret;

    private readonly string _deviceId;

    private DateTimeOffset _expires;

    public RedditAuthenticator(Credentials credentials) : base(string.Empty)
    {
        if (credentials is null
            || string.IsNullOrWhiteSpace(credentials.ClientId)
            || string.IsNullOrWhiteSpace(credentials.ClientSecret)
            || string.IsNullOrWhiteSpace(credentials.DeviceId))
        {
            throw new ArgumentNullException(nameof(credentials));
        }

        this._clientId = credentials.ClientId;
        this._clientSecret = credentials.ClientSecret;
        this._deviceId = credentials.DeviceId;
    }

    protected override async ValueTask<Parameter> GetAuthenticationParameter(string accessToken)
    {
        if (string.IsNullOrEmpty(this.Token) || this._expires <= DateTimeOffset.Now)
        {
            await this.GetAuthenticationAsync();
        }

        return new HeaderParameter(KnownHeaders.Authorization, this.Token);
    }

    private async Task GetAuthenticationAsync()
    {
        var options = new RestClientOptions(UrlStrings.RedditBaseUrl)
        {
            Authenticator = new HttpBasicAuthenticator(this._clientId, this._clientSecret)
        };
        var client = new RestClient(options);
        var request = new RestRequest(UrlStrings.TokenPartialUrl)
            .AddParameter(HeaderStrings.GrantTypeKey, HeaderStrings.GrantTypeValue)
            .AddParameter(HeaderStrings.DeviceIdKey, this._deviceId);
        var response = await client.PostAsync(request);

        if (!response.IsSuccessStatusCode
            || response.Content is null)
        {
            throw new Exception($"Unable to authenticate with response {(int)response.StatusCode} {response.StatusDescription}");
        }

        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(response.Content);

        this.Token = $"{tokenResponse!.TokenType} {tokenResponse!.AccessToken}";
        this._expires = tokenResponse.CreatedAt.AddSeconds(tokenResponse.ExpiresIn);
    }
}