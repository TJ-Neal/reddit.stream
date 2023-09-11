using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
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

    public Task AddPostsAsync(IEnumerable<Link> posts)
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

    public Task<List<Link>> GetAllPostsAsync(string? subreddit, Pagination pagination)
    {
        var posts = this.posts
            .Values
            .AsEnumerable();

        if (!string.IsNullOrWhiteSpace(subreddit))
        {
            posts = posts
                .Where(post => string.Equals(post.Subreddit, subreddit, StringComparison.InvariantCultureIgnoreCase));
        }

        return Task.FromResult(posts
            .Skip(pagination.PageSize * (pagination.Page - 1))
            .Take(pagination.PageSize)
            .ToList());
    }        

    public Task<IEnumerable<KeyValuePair<string, List<Link>>>> GetAllAuthorsAsync(string? subreddit, Pagination pagination)
    {
        var authors = this.authors
            .Values
            .AsEnumerable();

        if (!string.IsNullOrWhiteSpace(subreddit))
        {
            authors = authors
                .Where(posts => posts
                    .Any(post => string.Equals(post.Subreddit, subreddit, StringComparison.InvariantCultureIgnoreCase)));
        }

        return Task.FromResult(this.authors
            .Skip(pagination.PageSize * (pagination.Page - 1))
            .Take(pagination.PageSize));
    }

    public Task<long> GetPostsCountAsync(string? subreddit)
    {
        if (string.IsNullOrWhiteSpace(subreddit))
        {
            return Task.FromResult((long)this.posts.Count);
        }

        var posts = this.posts
            .Values
            .AsEnumerable();

        return Task.FromResult(
            (long)posts
                .Where(post => string.Equals(post.Subreddit, subreddit, StringComparison.InvariantCultureIgnoreCase))
                .Count());
    }

    public Task<long> GetAuthorsCountAsync(string? subreddit)
    {
        if (string.IsNullOrWhiteSpace(subreddit))
        {
            return Task.FromResult((long)this.authors.Count);
        }

        var authors = this.authors
            .Values
            .AsEnumerable();

        return Task.FromResult(
            (long)authors
                .Where(posts => posts
                    .Any(post => string.Equals(post.Subreddit, subreddit, StringComparison.InvariantCultureIgnoreCase)))
                .Count());
    }
    public Task<IEnumerable<KeyValuePair<string, int>>> GetTopPosts(string? subreddit, int top = 10)
    {
        var filteredPosts = string.IsNullOrWhiteSpace(subreddit)
            ? this.posts
                .Select(post => new KeyValuePair<string, int>(post.Key, post.Value.Ups))
            : this.posts
                .Where(post => string.Equals(post.Value.Subreddit, subreddit, StringComparison.InvariantCultureIgnoreCase))
                .Select(post => new KeyValuePair<string, int>(post.Key, post.Value.Ups));

        return Task.FromResult(filteredPosts
            .OrderByDescending(post => post.Value)
            .ThenBy(post => post.Key)
            .Take(top));
    }

    public Task<IEnumerable<KeyValuePair<string, int>>> GetTopAuthors(string? subreddit, int top = 10)
    {
        var filteredAuthors = string.IsNullOrEmpty(subreddit)
            ? this.authors
                .Select(author => new KeyValuePair<string, int>(author.Key, author.Value.Count))
            : this.authors
                .Select(author =>
                    new KeyValuePair<string, int>(
                        author.Key,
                        author.Value
                            .Where(link => string.Equals(link.Subreddit, subreddit, StringComparison.InvariantCultureIgnoreCase))
                            .Count()));

        return Task.FromResult(filteredAuthors
            .OrderByDescending(author => author.Value)
            .ThenBy(author => author.Key)
            .Take(top));
    }

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
