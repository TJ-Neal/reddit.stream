using FluentAssertions;
using Neal.Reddit.Client.Tests.TestFixtures;
using Neal.Reddit.Core.Entities.Reddit;

namespace Neal.Reddit.Client.Tests.Clients;

public class RedditClientTests : IClassFixture<RedditClientFixture>
{
    private readonly RedditClientFixture _fixture;

    public RedditClientTests(RedditClientFixture fixture) =>
        this._fixture = fixture;

    [Fact]
    public async Task RedditClient_GetSubredditPostsNewAsync_Success()
    {
        // Arrange
        var client = this._fixture.Client;

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