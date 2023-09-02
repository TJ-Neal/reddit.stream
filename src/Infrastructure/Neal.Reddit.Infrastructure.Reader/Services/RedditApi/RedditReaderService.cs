using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Neal.Reddit.Application.Constants.Messages;
using Neal.Reddit.Client.Models;
using Reddit.Controllers.EventArgs;

namespace Neal.Reddit.Infrastructure.Reader.Services.RedditApi;

public class RedditReaderService : BackgroundService
{
    private readonly ILogger<RedditReaderService> _logger;

    private readonly Credentials _credentials;

    public RedditReaderService(
        ILogger<RedditReaderService> logger,
        Credentials credentials)
    {
        _logger = logger;
        _credentials = credentials;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(CommonLogMessages.StartingLoop);

        try
        {
            //var accessToken = await RedditAuthenticator.GetClientRefreshTokenAsync(
            //    this._redditCredentials,
            //    cancellationToken);
            //var redditClient = new RedditClient(
            //    appId: this._redditCredentials.ClientId,
            //    accessToken: accessToken); // TODO: Move to Reddit wrapper
            //var subreddit = redditClient.Subreddit("Gaming");

            //subreddit.Posts.GetNew();
            //subreddit.Posts.MonitorNew();
            //subreddit.Posts.NewUpdated += NewPostHandler;

            //while (!cancellationToken.IsCancellationRequested)
            //{
            //    await Task.Run(() => { }, cancellationToken);
            //}

            //this._logger.LogInformation(CommonLogMessages.CancelRequested);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ExceptionMessages.ErrorDuringLoop, ex);
        }
    }

    private void NewPostHandler(object? _, PostsUpdateEventArgs e)
    {
        _logger.LogInformation(JsonSerializer.Serialize(e.OldPosts));
        _logger.LogInformation(JsonSerializer.Serialize(e.NewPosts));
    }
}