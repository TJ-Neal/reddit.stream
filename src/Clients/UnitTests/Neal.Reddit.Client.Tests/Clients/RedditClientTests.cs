using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Neal.Reddit.Client.Models;
using Neal.Reddit.Core.Entities.Reddit;

namespace Neal.Reddit.Client.Tests.Clients;

public class RedditClientTests
{
    private readonly Credentials _credentials;

    public RedditClientTests()
    {
        // TODO: Create fixture
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<RedditAuthenticatorTests>()
            .Build();

        this._credentials = configuration
            .GetSection(nameof(Credentials))
            ?.Get<Credentials>()
            ?? new Credentials();
    }

    [Fact]
    public async Task RedditClient_GetSubredditPostsNewAsync_Success()
    {
        // Arrange
        var authenticator = new RedditAuthenticator(this._credentials);
        var client = new RedditClient(authenticator);

        // Act
        var result = await client.GetSubredditPostsNewAsync("gaming");

        // Assert
        result.Should().NotBeNull();
        result!.Root.Should().NotBeNull();
        result!.Root!.Kind.Should().Be(Kind.Listing);
        result.Root.Data.Should().NotBeNull();
        result!.Root.Data!.Children.Should().NotBeNull();
        result.Root.Data.Children.Should().NotBeEmpty();
    }
}