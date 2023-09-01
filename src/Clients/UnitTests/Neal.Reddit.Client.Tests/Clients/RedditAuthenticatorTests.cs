using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Neal.Reddit.Client.Models;
using Neal.Reddit.Client.Tests.Helpers;
using RestSharp;

namespace Neal.Reddit.Client.Tests.Clients;

public class RedditAuthenticatorTests
{
    private readonly Credentials _credentials;

    public RedditAuthenticatorTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<RedditAuthenticatorTests>()
            .Build();

        this._credentials = configuration
            .GetSection(nameof(Credentials))
            ?.Get<Credentials>()
            ?? new Credentials();
    }

    [Fact]
    public async Task Reddit_Authentication_GetClientRefreshTokenAsync_Success()
    {
        if (this._credentials == default)
        {
            throw new InvalidOperationException("User secrets must be configured for Reddit Credentials");
        }

        // Arrange
        var authenticator = new RedditAuthenticatorHelper(this._credentials);

        // Act
        var result = await authenticator.GetAuthenticationParameter();

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(KnownHeaders.Authorization);
        result.Value.Should().NotBeNull();
        result.Value?.ToString().Should().Contain("bearer");
    }
}