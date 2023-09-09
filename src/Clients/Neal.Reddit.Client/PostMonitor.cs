using Neal.Reddit.Client.Interfaces;
using Neal.Reddit.Client.Models;
using Neal.Reddit.Core.Entities.Exceptions;
using Neal.Reddit.Core.Entities.Reddit;
using System.Collections.Concurrent;

namespace Neal.Reddit.Client;

internal class PostMonitor
{
    private readonly IRedditClient _redditClient;

    private readonly SubredditConfiguration _configuration;

    private readonly CancellationToken _cancellationToken;

    private readonly Func<Link, Task> _postHandler;

    private ConcurrentDictionary<string, int> WatchedPosts { get; } = new();

    private long StartEpochSeconds { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    private string StartingPost { get; set; } = string.Empty;

    internal PostMonitor(
        IRedditClient redditClient, 
        SubredditConfiguration configuration,
        Func<Link, Task> postHandler,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(configuration.Name))
        {
            throw new ConfigurationException<SubredditConfiguration>();
        }

        _redditClient = redditClient;
        _configuration = configuration;
        _postHandler = postHandler;
        _cancellationToken = cancellationToken;
    }

    internal async Task StartAsync()
    {
        while (!_cancellationToken.IsCancellationRequested)
        {
            await GetPostsAsync();
        }
    }

    private async Task GetPostsAsync()
    {
        int pertinentPostCount = 0;
        var paginationPost = string.Empty;

        do
        {
            var response = await _redditClient
                .GetPostsNewAsync(
                    _configuration,
                    this.StartingPost,
                    paginationPost);
            var posts = response?.Root?.Data?.Children ?? Enumerable.Empty<DataContainer<Link>>();

            foreach (var post in posts)
            {
                if (_configuration.AfterStartOnly
                    && post.Data?.CreatedUtcEpoch < this.StartEpochSeconds)
                {
                    this.StartingPost = post.Data.Name;

                    break;
                }
                else if (post.Data is not null)
                {
                    pertinentPostCount++;

                    var lastPost = response?.Root?.Data?.Children?.LastOrDefault();
                    paginationPost = lastPost?.Data?.Name ?? string.Empty;

                    var shouldUpdate = this.WatchedPosts.TryGetValue(post.Data.Name, out int upvotes)
                        ? upvotes < post.Data.Ups
                        : this.WatchedPosts.TryAdd(post.Data.Name, post.Data.Ups);

                    if (shouldUpdate)
                    {
                        await _postHandler(post.Data);
                    }
                }
            }

            // TODO: Replace with calculated sleep
            Thread.Sleep(1500);
        }
        while (pertinentPostCount == _configuration.PerRequestLimit);
    }
}
