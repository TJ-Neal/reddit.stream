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

    private readonly IConfiguration _configuration;

    private readonly ILogger<RedditReaderService> _logger;

    private readonly IRedditClient _redditClient;

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
                var tasks = new List<Task>();

                foreach (var subreddit in subreddits)
                {
                    tasks.Add(this._redditClient.MonitorSubredditPostsAsync(subreddit, cancellationToken));
                }

                await Task.WhenAll(tasks);
            }

            this._logger.LogInformation(CommonLogMessages.CancelRequested);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ExceptionMessages.ErrorDuringLoop, ex);
        }
    }
}