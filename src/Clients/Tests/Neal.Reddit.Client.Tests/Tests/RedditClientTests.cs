using FluentAssertions;
using Neal.Reddit.Client.Tests.TestFixtures;
using Neal.Reddit.Core.Entities.Configuration;
using Neal.Reddit.Core.Entities.Exceptions;
using Neal.Reddit.Core.Enums;
using System.Net;

namespace Neal.Reddit.Client.Tests.Tests;

public class RedditClientTests : IClassFixture<RedditClientFixture>
{
    private readonly RedditClientFixture _fixture;

    public RedditClientTests(RedditClientFixture fixture) =>
        _fixture = fixture;

    [Fact]
    [Trait("Category", "Reddit")]
    [Trait("Category", "Posts")]
    [Trait("Category", "Integration")]
    public async Task RedditClient_GetPostsAsync_NameOnly_Configuration_Success()
    {
        // Arrange
        var client = _fixture.Client;
        var configuration = new SubredditConfiguration()
        {
            Name = "gaming"
        };

        // Act
        var result = await client.GetPostsAsync(configuration);

        // Assert
        result.Should().NotBeNull();
        result!.Root.Should().NotBeNull();
        result!.Root!.Kind.Should().Be(Kind.Listing);
        result.Root.Data.Should().NotBeNull();
        result!.Root.Data!.Children.Should().NotBeNull();
        result.Root.Data.Children.Should().NotBeEmpty();
        result.Root.Data.Children.Should().HaveCountLessThanOrEqualTo(configuration.PerRequestLimit);
    }

    [Fact]
    [Trait("Category", "Reddit")]
    [Trait("Category", "Posts")]
    [Trait("Category", "Integration")]
    public async Task RedditClient_GetPostsAsync_Full_Configuration_Success()
    {
        // Arrange
        var client = _fixture.Client;
        var configuration = new SubredditConfiguration()
        {
            Name = "gaming",
            AfterStartOnly = true,
            PerRequestLimit = 50,
        };

        // Act
        var result = await client.GetPostsAsync(configuration);

        // Assert
        result.Should().NotBeNull();
        result!.Root.Should().NotBeNull();
        result!.Root!.Kind.Should().Be(Kind.Listing);
        result.Root.Data.Should().NotBeNull();
        result!.Root.Data!.Children.Should().NotBeNull();
        result.Root.Data.Children.Should().NotBeEmpty();
        result.Root.Data.Children.Should().HaveCountLessThanOrEqualTo(configuration.PerRequestLimit);
    }

    [Fact]
    [Trait("Category", "Reddit")]
    [Trait("Category", "Posts")]
    [Trait("Category", "Integration")]
    public async Task RedditClient_GetPostsAsync_Empty_Configuration_ShouldThrow()
    {
        // Arrange
        var client = _fixture.Client;
        var configuration = new SubredditConfiguration();

        // Act & Assert
        await Assert.ThrowsAsync<ConfigurationException<SubredditConfiguration>>(
            async () => await client.GetPostsAsync(configuration));
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
        Task asyncCallback()
        {
            cancellationSource.Cancel();
            callbackCalled = true;

            return Task.CompletedTask;
        }

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