using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Neal.Reddit.Client.Models;

namespace Neal.Reddit.Client.Tests.Clients;

public class RedditClientTests
{
    private readonly Credentials _credentials;

    public RedditClientTests()
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
    public async Task RedditClient_GetSubredditNewAsync_Success()
    {
        if (this._credentials == default)
        {
            throw new InvalidOperationException("User secrets must be configured for Reddit Credentials");
        }

        // Arrange
        var authenticator = new RedditAuthenticator(this._credentials);
        var client = new RedditClient(authenticator);

        // Act
        var result = await client.GetSubredditNewAsync("gaming");

        // Assert
        result.Should().NotBeNull();
    }
}