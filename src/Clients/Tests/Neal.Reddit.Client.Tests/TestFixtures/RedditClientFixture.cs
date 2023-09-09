using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Neal.Reddit.Client.Interfaces;
using Neal.Reddit.Client.Models;
using Neal.Reddit.Client.Tests.Tests;
using RestSharp.Authenticators;

namespace Neal.Reddit.Client.Tests.TestFixtures;

public class RedditClientFixture
{
    private readonly ILoggerFactory loggerFactory = LoggerFactory.Create(logger => logger.AddConsole());

    public IRedditClient Client { get; private set; }

    public RedditClientFixture()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<RedditAuthenticatorTests>()
            .Build();
        var credentials = configuration
            .GetSection(nameof(Credentials))
            ?.Get<Credentials>()
            ?? new Credentials();
        var authenticator = new RedditAuthenticator(credentials);

        this.Client = new ServiceCollection()
            .AddSingleton(this._loggerFactory.CreateLogger<RedditClient>())
            .AddSingleton<IAuthenticator>(authenticator)
            .AddSingleton(typeof(IRedditClient), typeof(RedditClient))
            .BuildServiceProvider()
            .GetService<IRedditClient>()
                ?? throw new InvalidOperationException($"Unable to create service collection for {nameof(IRedditClient)}.");
    }
}
