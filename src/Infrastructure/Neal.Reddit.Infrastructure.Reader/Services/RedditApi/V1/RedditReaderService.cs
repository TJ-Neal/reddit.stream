using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Neal.Reddit.Application.Constants.Messages;
using Newtonsoft.Json;
using Reddit;
using Reddit.Controllers.EventArgs;

namespace Neal.Reddit.Infrastructure.Reader.Services.RedditApi.V1;

public class RedditReaderService : BackgroundService
{
    private readonly ILogger<RedditReaderService> _logger;

    private readonly IHttpClientFactory clientFactory;

    public RedditReaderService(ILogger<RedditReaderService> logger)
    {
        this._logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        this._logger.LogInformation(CommonLogMessages.StartingLoop);

        try
        {
            var redditClient = new RedditClient("Tfwns6mC_l9tkXwtjRDTDw", );
            var subreddit = redditClient.Subreddit("Gaming");

            subreddit.Posts.GetNew();
            subreddit.Posts.MonitorNew();

            //while (!cancellationToken.IsCancellationRequested)
            //{
            //    await Task.Run(() => { }, cancellationToken);
            //}

            this._logger.LogInformation(CommonLogMessages.CancelRequested);
        }
        catch (Exception ex)
        {
            this._logger.LogCritical(ExceptionMessages.ErrorDuringLoop, ex);
        }
    }

    private string GetClientRefreshToken()
    {
    }

    private void NewPostHandler(object sender, PostUpdateEventArgs e)
    {
        this._logger.LogInformation(JsonConvert.SerializeObject(e.OldPost));
        this._logger.LogInformation(JsonConvert.SerializeObject(e.NewPost));
    }
}