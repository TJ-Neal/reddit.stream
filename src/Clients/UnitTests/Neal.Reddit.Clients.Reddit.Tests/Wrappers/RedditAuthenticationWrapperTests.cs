using FluentAssertions;
using Neal.Reddit.Clients.Reddit.Wrappers;

namespace Neal.Reddit.Clients.Reddit.Tests.Wrappers;

public class RedditAuthenticationWrapperTests
{
    [Fact]
    public async Task Reddit_Authentication_GetClientRefreshTokenAsync_Success()
    {
        // Arrange
        var cancellationSource = new CancellationTokenSource();
        var cancellationToken = cancellationSource.Token;
        var clientId = "Tfwns6mC_l9tkXwtjRDTDw";
        var clientSecret = "<SECRET>"; // TODO: Should be secret

        // Act
        var result = await RedditAuthenticationWrapper.GetClientRefreshTokenAsync(clientId, clientSecret, cancellationToken);

        // Assert
        result.Should().NotBeNull();
    }
}