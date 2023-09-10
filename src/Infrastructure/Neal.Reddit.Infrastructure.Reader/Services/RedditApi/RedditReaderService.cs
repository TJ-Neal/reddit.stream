using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Neal.Reddit.Application.Constants.Messages;
using Neal.Reddit.Application.Events.Notifications;
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

    private readonly IMediator mediator;

    public RedditReaderService(
        IConfiguration configuration,
        ILogger<RedditReaderService> logger,
        IRedditClient redditClient,
        IMediator mediator)
    {
        this.configuration = configuration;
        this.logger = logger;
        this.redditClient = redditClient;
        this.mediator = mediator;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation(CommonLogMessages.StartingLoop);

        try
        {
            var subreddits = this.configuration
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
                    this.redditClient.GetPostsAsync(
                        subreddit,
                        this.HandleNewPostAsync,
                        cancellationToken));
            }

            await Task.WhenAll(tasks);

            this.logger.LogInformation(CommonLogMessages.CancelRequested);
        }
        catch (Exception ex)
        {
            this.logger.LogCritical(ExceptionMessages.ErrorDuringLoop, ex);
        }
    }

    private async Task HandleNewPostAsync(Link post, CancellationToken cancellationToken)
    {
        this.logger.LogInformation(
            "Post {postName} in {subreddit} has {upvotes} ups.",
            post.Name,
            post.Subreddit,
            post.Ups);

        await this.mediator
            .Publish(new PostReceivedOrUpdatedNotification(post), cancellationToken);
    }
}