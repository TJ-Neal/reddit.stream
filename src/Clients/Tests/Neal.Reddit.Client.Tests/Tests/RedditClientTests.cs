using FluentAssertions;
using Neal.Reddit.Client.Tests.Helpers;
using Neal.Reddit.Client.Tests.TestFixtures;
using Neal.Reddit.Core.Entities.Configuration;
using Neal.Reddit.Core.Entities.Exceptions;
using Neal.Reddit.Core.Entities.Reddit;

namespace Neal.Reddit.Client.Tests.Tests;

public class RedditClientTests : IClassFixture<RedditClientFixture>
{
    private readonly RedditClientFixture fixture;

    public RedditClientTests(RedditClientFixture fixture) =>
        this.fixture = fixture;

    [Theory]
    [Trait("Category", "Reddit")]
    [Trait("Category", "Posts")]
    [Trait("Category", "Integration")]
    [ClassData(typeof(ClientTestData))]
    public async Task RedditClient_GetPostsAsync_Configuration_Success(SubredditConfiguration configuration)
    {
        // Arrange
        var client = this.fixture.Client;
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var posts = new List<Link>();
        bool callbackCalled = false;
        Task asyncCallback(Link post, CancellationToken cancelToken)
        {
            posts.Add(post);

            cancellationTokenSource.Cancel();
            callbackCalled = true;

            return Task.CompletedTask;
        }
        
        if (string.IsNullOrWhiteSpace(configuration.Name))
        {
            // Act & Assert
            await Assert.ThrowsAsync<ConfigurationException<SubredditConfiguration>>(
                async () => await client.GetPostsAsync(configuration, asyncCallback, cancellationToken));
        }
        else
        {
            // Act
            await Assert.ThrowsAsync<TaskCanceledException>(
                async () => await client.GetPostsAsync(configuration, asyncCallback, cancellationToken));

            // Assert
            callbackCalled.Should().BeTrue();
            posts.Should().NotBeEmpty();
        }
    }
}