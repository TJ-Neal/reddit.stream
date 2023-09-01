using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Neal.Reddit.Application.Constants.Messages;
using Neal.Reddit.Clients.Reddit.Wrappers;
using Neal.Reddit.Core.Entities.Configuration;
using Newtonsoft.Json;
using Reddit;
using Reddit.Controllers.EventArgs;

namespace Neal.Reddit.Infrastructure.Reader.Services.RedditApi.V1;

public class RedditReaderService : BackgroundService
{
    private readonly ILogger<RedditReaderService> _logger;

    private readonly RedditCredentials _redditCredentials;

    public RedditReaderService(
        ILogger<RedditReaderService> logger,
        RedditCredentials redditCredentials)
    {
        this._logger = logger;
        this._redditCredentials = redditCredentials;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        this._logger.LogInformation(CommonLogMessages.StartingLoop);

        try
        {
            var accessToken = await RedditAuthenticationWrapper.GetClientRefreshTokenAsync(
                this._redditCredentials,
                cancellationToken);
            var redditClient = new RedditClient(
                appId: this._redditCredentials.ClientId,
                accessToken: accessToken); // TODO: Move to Reddit wrapper
            var subreddit = redditClient.Subreddit("Gaming");

            subreddit.Posts.GetNew();
            subreddit.Posts.MonitorNew();
            subreddit.Posts.NewUpdated += NewPostHandler;

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Run(() => { }, cancellationToken);
            }

            this._logger.LogInformation(CommonLogMessages.CancelRequested);
        }
        catch (Exception ex)
        {
            this._logger.LogCritical(ExceptionMessages.ErrorDuringLoop, ex);
        }
    }

    private void NewPostHandler(object? sender, PostsUpdateEventArgs e)
    {
        this._logger.LogInformation(JsonConvert.SerializeObject(e.OldPosts));
        this._logger.LogInformation(JsonConvert.SerializeObject(e.NewPosts));
    }
}