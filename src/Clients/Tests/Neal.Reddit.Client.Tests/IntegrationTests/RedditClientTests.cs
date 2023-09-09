using FluentAssertions;
using Neal.Reddit.Client.Models;
using Neal.Reddit.Client.Tests.TestFixtures;
using Neal.Reddit.Core.Entities.Reddit;

namespace Neal.Reddit.Client.Tests.Clients;

public class RedditClientTests : IClassFixture<RedditClientFixture>
{
    private readonly RedditClientFixture _fixture;

    public RedditClientTests(RedditClientFixture fixture) =>
        _fixture = fixture;
        
    [Fact]
    [Trait("Category", "Reddit")]
    [Trait("Category", "Posts")]
    [Trait("Category", "Integration")]
    public async Task RedditClient_GetPostsNewAsync_Success()
    {
        // Arrange
        var client = _fixture.Client;
        var configuration = new SubredditConfiguration()
        {
            Name = "gaming",
            AfterStartOnly = true
        };

        // Act
        var result = await client.GetPostsNewAsync(configuration);

        // Assert
        result.Should().NotBeNull();
        result!.Root.Should().NotBeNull();
        result!.Root!.Kind.Should().Be(Kind.Listing);
        result.Root.Data.Should().NotBeNull();
        result!.Root.Data!.Children.Should().NotBeNull();
        result.Root.Data.Children.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Reddit")]
    [Trait("Category", "Posts")]
    [Trait("Category", "Monitoring")]
    [Trait("Category", "Integration")]
    public async Task RedditClient_MonitorPostsAysnc_Success()
    {
        // Arrange
        var client = _fixture.Client;
        var cancellationSource = new CancellationTokenSource();
        var cancellationToken = cancellationSource.Token;
        var configuration = new SubredditConfiguration()
        {
            Name = "gaming",
            AfterStartOnly = false
        };
        bool callbackCalled = false;
        var asyncCallback = delegate ()
        {
            cancellationSource.Cancel();
            callbackCalled = true;

            return Task.CompletedTask;
        };

        // Act
        // TODO: Call monitor with callback as delegate

        // Assert
    }

    [Fact]
    [Trait("Category", "Reddit")]
    [Trait("Category", "Posts")]
    [Trait("Category", "Monitoring")]
    [Trait("Category", "Integration")]
    public async Task RedditClient_MonitorPostsAysnc_AfterStartOnly_Success()
    {
        // Arrange
        var client = _fixture.Client;
        var cancellationSource = new CancellationTokenSource();
        var cancellationToken = cancellationSource.Token;
        var configuration = new SubredditConfiguration()
        {
            Name = "gaming",
            AfterStartOnly = true
        };
        bool callbackCalled = false;
        var asyncCallback = delegate ()
        {
            cancellationSource.Cancel();
            callbackCalled = true;

            return Task.CompletedTask;
        };

        // Act
        // TODO: Call monitor with callback as delegate

        // Assert
    }
}