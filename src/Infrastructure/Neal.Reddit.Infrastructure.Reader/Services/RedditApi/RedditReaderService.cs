using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Neal.Reddit.Application.Constants.Messages;
using Neal.Reddit.Client.Interfaces;
using Neal.Reddit.Core.Entities.Configuration;
using Neal.Reddit.Core.Entities.Exceptions;
using Neal.Reddit.Core.Entities.Reddit;

namespace Neal.Reddit.Infrastructure.Reader.Services.RedditApi;

public class RedditReaderService : BackgroundService
{

    private readonly IConfiguration configuration;

    private readonly ILogger<RedditReaderService> logger;

    private readonly IRedditClient redditClient;

    public RedditReaderService(
        IConfiguration configuration,
        ILogger<RedditReaderService> logger,
        IRedditClient redditClient)
    {
        this.configuration = configuration;
        this.logger = logger;
        this.redditClient = redditClient;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation(CommonLogMessages.StartingLoop);

        try
        {
            var subreddits = configuration
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
                tasks.Add(
                    redditClient.GetPostsAsync(
                        subreddit,
                        HandleNewPostAsync,
                        cancellationToken));
            }

            await Task.WhenAll(tasks);

            logger.LogInformation(CommonLogMessages.CancelRequested);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ExceptionMessages.ErrorDuringLoop, ex);
        }
    }

    private async Task HandleNewPostAsync(Link post)
    {
        logger.LogInformation("Post {post}", post);
    }
}