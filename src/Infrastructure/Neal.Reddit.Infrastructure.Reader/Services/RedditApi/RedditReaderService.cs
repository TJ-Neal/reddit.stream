using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Neal.Reddit.Application.Constants.Messages;
using Neal.Reddit.Client.Interfaces;
using Neal.Reddit.Client.Models;
using Neal.Reddit.Core.Entities.Exceptions;
using Neal.Reddit.Core.Entities.Reddit;

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
                .GetSection(nameof(SubredditConfiguration))
                ?.Get<List<SubredditConfiguration>>();

            if (subreddits is null
                || subreddits.Count < 1)
            {
                throw new ConfigurationException<SubredditConfiguration>();
            }

            var tasks = new List<Task>();

            foreach (var subreddit in subreddits)
            {
                tasks
                    .Add(_redditClient
                        .MonitorPostsAsync(
                            subreddit, 
                            HandleNewPostAsync,
                            cancellationToken));
            }

            await Task.WhenAll(tasks);

            _logger.LogInformation(CommonLogMessages.CancelRequested);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ExceptionMessages.ErrorDuringLoop, ex);
        }
    }

    private async Task HandleNewPostAsync(Link post)
    {
        _logger.LogInformation("Post {post}", post);
    }
}