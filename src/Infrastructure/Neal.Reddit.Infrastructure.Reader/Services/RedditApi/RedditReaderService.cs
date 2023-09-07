using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Neal.Reddit.Application.Constants.Messages;
using Neal.Reddit.Client.Interfaces;
using Reddit.Models;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Neal.Reddit.Infrastructure.Reader.Services.RedditApi;

public class RedditReaderService : BackgroundService
{
    private const int LIMIT = 100;

    private readonly IConfiguration _configuration;

    private readonly ILogger<RedditReaderService> _logger;

    private readonly IRedditClient _redditClient;

    private readonly List<Thread> _threads = new();

    public RedditReaderService(
        IConfiguration configuration,
        ILogger<RedditReaderService> logger,
        IRedditClient redditClient)
    {
        _configuration = configuration;
        _logger = logger;
        _redditClient = redditClient;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(CommonLogMessages.StartingLoop);

        try
        {
            var subreddits = _configuration
                .GetSection("Subreddits")
                ?.Get<string[]>();

            if (subreddits is null
                || subreddits.Length < 1)
            {
                return;
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                foreach (var subreddit in subreddits)
                {
                    var count = 0;
                    var after = string.Empty;

                    do
                    {                        
                        Debug.WriteLine($"After: {after}");
                        var posts = await this._redditClient.GetSubredditPostsNewAsync(subreddit, after: after);
                        
                        var firstPost = posts?.Root?.Data?.Children?.FirstOrDefault();
                        var lastPost = posts?.Root?.Data?.Children?.LastOrDefault();
                        var firstEpoch = DateTimeOffset.FromUnixTimeSeconds((int?)firstPost?.Data?.CreatedUtcEpoch ?? 0);
                        var lastEpoch = DateTimeOffset.FromUnixTimeSeconds((int?)lastPost?.Data?.CreatedUtcEpoch ?? 0);
                        Debug.WriteLine($"Post for {subreddit} : {posts?.Root?.Data?.Count} : First {firstPost?.Data?.Name} {firstEpoch} : Last {lastPost?.Data?.Name} {lastEpoch}");
                        count = posts?.Root?.Data?.Count ?? 0;
                        after = lastPost?.Data?.Name ?? string.Empty;
                    }
                    while (count == LIMIT);

                    // TODO: Call handler
                }

                Thread.Sleep(1500);
            }

            this._logger.LogInformation(CommonLogMessages.CancelRequested);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ExceptionMessages.ErrorDuringLoop, ex);
        }
    }
}