using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Neal.Reddit.Application.Constants.Messages;
using Neal.Reddit.Client.Models;
using Newtonsoft.Json;
using Reddit.Controllers.EventArgs;

namespace Neal.Reddit.Infrastructure.Reader.Services.RedditApi.V1;

public class RedditReaderService : BackgroundService
{
    private readonly ILogger<RedditReaderService> _logger;

    private readonly Credentials _credentials;

    public RedditReaderService(
        ILogger<RedditReaderService> logger,
        Credentials credentials)
    {
        this._logger = logger;
        this._credentials = credentials;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        this._logger.LogInformation(CommonLogMessages.StartingLoop);

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
            this._logger.LogCritical(ExceptionMessages.ErrorDuringLoop, ex);
        }
    }

    private void NewPostHandler(object? sender, PostsUpdateEventArgs e)
    {
        this._logger.LogInformation(JsonConvert.SerializeObject(e.OldPosts));
        this._logger.LogInformation(JsonConvert.SerializeObject(e.NewPosts));
    }
}