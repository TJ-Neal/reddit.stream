using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Neal.Reddit.Application.Constants.Messages;
using Neal.Reddit.Client.Interfaces;
using Reddit.Controllers.EventArgs;

namespace Neal.Reddit.Infrastructure.Reader.Services.RedditApi;

public class RedditReaderService : BackgroundService
{
    private readonly ILogger<RedditReaderService> _logger;

    private readonly IRedditClient _redditClient;

    private readonly List<Thread> _threads = new();

    public RedditReaderService(
        ILogger<RedditReaderService> logger,
        IRedditClient redditClient)
    {
        _logger = logger;
        _redditClient = redditClient;
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

    //public async Task MonitorSubredditsForNewAsync(
    //    IEnumerable<string> subreddits,
    //    CancellationToken cancellationToken)
    //{
    //    var subredditCount = subreddits?.ToList().Count;

    //    if (subreddits is null
    //        || !subredditCount.HasValue
    //        || subredditCount <= 0)
    //    {
    //        return;
    //    }

    //    var lastPostDictionary = new ConcurrentDictionary<string, string?>();

    //    while (!cancellationToken.IsCancellationRequested)
    //    {
    //        foreach (var subreddit in subreddits)
    //        {
    //            _ = lastPostDictionary.TryGetValue(subreddit, out var before);
    //            var posts = await GetSubredditPostsNewAsync(subreddit, before ?? string.Empty);
    //            var newBefore = posts?.Data.Children?.FirstOrDefault()?.Data?.Name ?? before;

    //            lastPostDictionary.AddOrUpdate(
    //                subreddit,
    //                newBefore, 
    //                (key, _) => newBefore);

    //            var firstEpoch = DateTimeOffset.FromUnixTimeSeconds((int?)posts?.Data.Children?.FirstOrDefault()?.Data?.CreatedUtcEpoch ?? 0);
    //            var lastEpoch = DateTimeOffset.FromUnixTimeSeconds((int?)posts?.Data.Children?.LastOrDefault()?.Data?.CreatedUtcEpoch ?? 0);
    //            Debug.WriteLine($"Post for {subreddit} : {posts?.Data.Count} : First {firstEpoch} : Last {lastEpoch} : {newBefore}");

    //            // TODO: Call handler

    //            Thread.Sleep(GetRequestDelay(subredditCount.Value));
    //        }
    //    }
    //}

    private void NewPostHandler(object? _, PostsUpdateEventArgs e)
    {
        _logger.LogInformation(JsonSerializer.Serialize(e.OldPosts));
        _logger.LogInformation(JsonSerializer.Serialize(e.NewPosts));
    }
}