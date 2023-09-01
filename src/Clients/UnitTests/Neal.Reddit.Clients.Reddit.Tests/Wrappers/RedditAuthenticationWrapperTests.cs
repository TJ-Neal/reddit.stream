using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Neal.Reddit.Clients.Reddit.Wrappers;
using Neal.Reddit.Core.Entities.Configuration;

namespace Neal.Reddit.Clients.Reddit.Tests.Wrappers;

public class RedditAuthenticationWrapperTests
{
    private RedditCredentials _redditCredentials;

    public RedditAuthenticationWrapperTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<RedditAuthenticationWrapperTests>()
            .Build();

        this._redditCredentials = configuration
        .GetSection(nameof(RedditCredentials))
        .Get<RedditCredentials>()
            ?? new RedditCredentials();
    }   

    [Fact]
    public async Task Reddit_Authentication_GetClientRefreshTokenAsync_Success()
    {
        // Arrange
        var cancellationSource = new CancellationTokenSource();
        var cancellationToken = cancellationSource.Token;

        // Act
        var result = await RedditAuthenticationWrapper.GetClientRefreshTokenAsync(this._redditCredentials, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain(
            "access_token", 
            $"User Secrets need to be set to valid {nameof(RedditCredentials)} shape. See README");
    }
}