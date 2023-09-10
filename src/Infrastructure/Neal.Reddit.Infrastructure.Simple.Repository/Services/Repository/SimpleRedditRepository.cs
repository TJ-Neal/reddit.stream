using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Neal.Reddit.Application.Constants.Keys;
using Neal.Reddit.Application.Constants.Messages;
using Neal.Reddit.Application.Interfaces.RedditRepository;
using Neal.Reddit.Core.Entities.Configuration;
using Neal.Reddit.Core.Entities.Reddit;
using System.Collections.Concurrent;

namespace Neal.Reddit.Infrastructure.Simple.Repository.Services.Repository;

public class SimpleRedditRepository : IPostRepository
{
    #region Fields

    private readonly IMemoryCache memoryCache;

    private readonly ILogger<SimpleRedditRepository> logger;

    private readonly Thread heartbeatThread;

    private readonly CancellationTokenSource cancellationTokenSource;

    private ConcurrentDictionary<string, Link> Posts
    {
        get
        {
            var cachedValue = this.memoryCache
                .GetOrCreate<ConcurrentDictionary<string, Link>>(
                    CacheKeys.PostsRepository,
                    cacheEntry =>
                    {
                        cacheEntry.SlidingExpiration = TimeSpan.FromHours(1);

                        return new();
                    });

            return cachedValue ?? new();
        }
    }

    private ConcurrentDictionary<string, int> PostUps
    {
        get
        {
            var cachedValue = this.memoryCache
                .GetOrCreate<ConcurrentDictionary<string, int>>(
                    CacheKeys.PostsRepository,
                    cacheEntry =>
                    {
                        cacheEntry.SlidingExpiration = TimeSpan.FromHours(1);

                        return new();
                    });

            return cachedValue ?? new();
        }
    }

    private ConcurrentDictionary<string, int> Authors
    {
        get
        {
            var cachedValue = this.memoryCache
                .GetOrCreate<ConcurrentDictionary<string, int>>(
                    CacheKeys.PostsRepository,
                    cacheEntry =>
                    {
                        cacheEntry.SlidingExpiration = TimeSpan.FromHours(1);

                        return new();
                    });

            return cachedValue ?? new();
        }
    }

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

    public Task AddPostsAsync(IEnumerable<Link> posts)
    {
        if (posts is null)
        {
            return Task.CompletedTask;
        }

        foreach (var post in posts)
        {
            if (this.Posts.TryAdd(post.Name, post))
            {                
                this.Authors.AddOrUpdate(post.AuthorName, 1, (_, old) => ++old);
            }

            this.PostUps.AddOrUpdate(post.Name, post.Ups, (_, _) => post.Ups);
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        this.logger.LogInformation(CommonLogMessages.Disposing, nameof(SimpleRedditRepository));

        this.cancellationTokenSource.Cancel();
        this.Posts.Clear();
        this.PostUps.Clear();
        this.Authors.Clear();
        this.memoryCache.Dispose();

        GC.SuppressFinalize(this);
    }

    public Task<List<Link>> GetAllPostsAsync(Pagination pagination) =>
        Task.FromResult(this.Posts
            .Values
            .AsEnumerable()
            .Skip(pagination.PageSize * (pagination.Page - 1))
            .Take(pagination.PageSize)
            .ToList());

    public Task<List<KeyValuePair<string, int>>> GetAllAuthors(Pagination pagination) =>
        Task.FromResult(this.Authors
            .AsEnumerable()
            .Skip(pagination.PageSize * (pagination.Page - 1))
            .Take(pagination.PageSize)
            .ToList());

    public Task<long> GetPostsCountAsync() =>
        Task.FromResult((long)this.Posts.Count);

    public Task<long> GetAuthorsCountAsync() =>
        Task.FromResult((long)this.Authors.Count);

    public Task<IEnumerable<KeyValuePair<string, int>>> GetTopAuthors(int top = 10) =>
        Task.FromResult(this.Authors
            .OrderByDescending(author => author.Value)
            .ThenBy(author => author.Key)
            .Take(top));

    public Task<IEnumerable<KeyValuePair<string, int>>> GetTopPosts(int top = 10) =>
        Task.FromResult(this.PostUps
            .OrderByDescending(upvote => upvote.Value)
            .ThenBy(upvote => upvote.Key)
            .Take(top));

    private void HeartbeatThread(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            Thread.Sleep(TimeSpan.FromSeconds(30));

            this.logger.LogInformation(
                ApplicationStatusMessages.PostsCount,
                nameof(SimpleRedditRepository),
                this.Posts.Count,
                this.Authors.Count);
        }
    }
}
