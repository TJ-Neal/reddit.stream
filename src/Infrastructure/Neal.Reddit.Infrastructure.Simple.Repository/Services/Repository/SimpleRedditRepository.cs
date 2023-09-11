using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Neal.Reddit.Application.Constants.Messages;
using Neal.Reddit.Application.Interfaces.RedditRepository;
using Neal.Reddit.Core.Entities.Configuration;
using Neal.Reddit.Core.Entities.Reddit;
using Neal.Reddit.Infrastructure.Simple.Repository.Constants;
using System.Collections.Concurrent;

namespace Neal.Reddit.Infrastructure.Simple.Repository.Services.Repository;

public class SimpleRedditRepository : IPostRepository
{
    #region Fields

    private readonly IMemoryCache memoryCache;

    private readonly ILogger<SimpleRedditRepository> logger;

    private readonly Thread heartbeatThread;

    private readonly CancellationTokenSource cancellationTokenSource;

    private readonly ConcurrentDictionary<string, Link> posts = new();

    private readonly ConcurrentDictionary<string, List<Link>> authors = new();

    #endregion Fields

    public SimpleRedditRepository(
        IMemoryCache memoryCache,
        ILogger<SimpleRedditRepository> logger)
    {
        this.memoryCache = memoryCache;
        this.logger = logger;
        this.cancellationTokenSource = new CancellationTokenSource();

        // Ensure a commit thread is running once tweets have been added at least once
        if (this.heartbeatThread is null)
        {
            this.heartbeatThread = new Thread(new ThreadStart(() => this.HeartbeatThread(this.cancellationTokenSource.Token)));
            this.heartbeatThread.Start();
        }
    }

    public Task AddOrUpdatePostsAsync(IEnumerable<Link> posts)
    {
        if (posts is null)
        {
            return Task.CompletedTask;
        }

        foreach (var post in posts)
        {
            if (this.posts.TryAdd(post.Name, post))
            {                
                if (this.authors.TryGetValue(post.AuthorName, out var authorPosts))
                {
                    authorPosts.Add(post);
                }
                else
                {
                    this.authors
                        .TryAdd(post.AuthorName, new List<Link> { post });
                }
            }
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        this.logger.LogInformation(CommonLogMessages.Disposing, nameof(SimpleRedditRepository));

        this.cancellationTokenSource.Cancel();
        this.posts.Clear();
        this.authors.Clear();
        this.memoryCache.Dispose();

        GC.SuppressFinalize(this);
    }

    public async Task<IEnumerable<Link>> GetAllPostsAsync(string? subreddit, Pagination pagination) =>
        (await this.GetCachedPostsAsync(subreddit))
            .Skip(pagination.PageSize * (pagination.Page - 1))
            .Take(pagination.PageSize);

    public async Task<IEnumerable<Author>> GetAllAuthorsAsync(string? subreddit, Pagination pagination) =>
        (await this.GetCachedAuthorsAsync(subreddit))
            .Skip(pagination.PageSize * (pagination.Page - 1))
            .Take(pagination.PageSize);

    public async Task<long> GetPostsCountAsync(string? subreddit) =>
        (await this.GetCachedPostsAsync(subreddit))
            .Where(post => string.IsNullOrWhiteSpace(subreddit)
                || string.Equals(post.Subreddit, subreddit, StringComparison.InvariantCultureIgnoreCase))
            .Count();

    public async Task<long> GetAuthorsCountAsync(string? subreddit) =>
        (await this.GetCachedAuthorsAsync(subreddit))
            .Where(author => author.Posts
                .Any(post => string.IsNullOrWhiteSpace(subreddit)
                    || string.Equals(post.Subreddit, subreddit, StringComparison.InvariantCultureIgnoreCase)))
            .Count();

    public async Task<IEnumerable<Link>> GetTopPostsByUpvotesAsync(string? subreddit, int top = 10) =>
        (await this.GetCachedPostsAsync(subreddit))
            .OrderByDescending(post => post.Ups)
            .ThenBy(post => post.Name)
            .Take(top);

    public async Task<IEnumerable<Link>> GetTopPostsByCommentsAsync(string? subreddit, int top = 10) =>
        (await this.GetCachedPostsAsync(subreddit))
            .OrderByDescending(post => post.CommentCount)
            .ThenBy(post => post.Name)
            .Take(top);

    public async Task<IEnumerable<Link>> GetLowestPostsByUpvoteRatioAsync(string? subreddit, int top = 10) =>
        (await this.GetCachedPostsAsync(subreddit))
            .Where(post => post.Score > 0 || post.Downs > 0)
            .OrderBy(post => post.UpvoteRatio)
            .OrderBy(post => post.Score)
            .ThenBy(post => post.Name)
            .Take(top);

    public async Task<IEnumerable<Author>> GetTopAuthors(string? subreddit, int top = 10) =>
        (await this.GetCachedAuthorsAsync(subreddit))
            .OrderByDescending(author => author.Posts.Count())
            .ThenBy(author => author.Name)
            .Take(top);

    private async Task<List<Link>> GetCachedPostsAsync(string? subreddit) =>
        (await this.GetOrCreateFilteredCacheAsync(
            $"{CacheKeys.CacheRoot}-{CacheKeys.Posts}-{subreddit}",
            GetPostsBySubreddit,
            subreddit))
                ?? new();

    private async Task<List<Author>> GetCachedAuthorsAsync(string? subreddit) =>
        (await this.GetOrCreateFilteredCacheAsync(
            $"{CacheKeys.CacheRoot}-{CacheKeys.Authors}-{subreddit}",
            GetAuthorsBySubreddit,
            subreddit))
                ?? new();

    private async Task<TResult?> GetOrCreateFilteredCacheAsync<TResult, TFilter>(
        string cacheKey, 
        Func<TFilter, Task<TResult>> newValue, 
        TFilter filter) where TResult : class =>
        await this.memoryCache.GetOrCreateAsync(
            cacheKey,
            async cacheEntry =>
            {
                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);

                return await newValue(filter);
            });

    private Task<List<Link>> GetPostsBySubreddit(string? subreddit) =>
        Task.FromResult(
            (string.IsNullOrWhiteSpace(subreddit)
                ? this.posts.Values
                : this.posts
                    .Where(post => string.Equals(post.Value.Subreddit, subreddit, StringComparison.InvariantCultureIgnoreCase))
                    .Select(post => post.Value))
            .OrderBy(post => post.Name)
            .ToList());

    private Task<List<Author>> GetAuthorsBySubreddit(string? subreddit) =>
        Task.FromResult(
            (string.IsNullOrEmpty(subreddit)
                ? this.authors
                    .Select(author => new Author() { Name = author.Key, Posts = author.Value })
                : this.authors
                    .Select(author =>
                        new Author()
                        {
                            Name = author.Key,
                            Posts = author.Value
                                .Where(link => string.Equals(link.Subreddit, subreddit, StringComparison.InvariantCultureIgnoreCase)),
                        }))
            .OrderBy(auth => auth.Name)
            .ToList());

    private void HeartbeatThread(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            Thread.Sleep(TimeSpan.FromSeconds(30));

            this.logger.LogInformation(
                ApplicationStatusMessages.PostsCount,
                nameof(SimpleRedditRepository),
                this.posts.Count,
                this.authors.Count);
        }
    }
}
