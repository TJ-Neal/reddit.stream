﻿using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Neal.Reddit.Client.Models;
using Neal.Reddit.Client.Tests.Helpers;
using Neal.Reddit.Core.Entities.Exceptions;
using RestSharp;

namespace Neal.Reddit.Client.Tests.Tests;

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
    public void Reddit_Authentication_GetAuthenticationParameter_EmptyCredentials_ShouldExcept()
    {
        // Arrange
        var credentials = new Credentials();

        // Act & Assert
        Assert.Throws<ConfigurationException<Credentials>>(() => new RedditAuthenticator(credentials));
    }

    [Fact]
    public async Task Reddit_Authentication_GetAuthenticationParameter_Success()
    {
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