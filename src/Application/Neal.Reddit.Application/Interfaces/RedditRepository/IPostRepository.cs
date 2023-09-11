using Neal.Reddit.Core.Entities.Configuration;
using Neal.Reddit.Core.Entities.Reddit;

namespace Neal.Reddit.Application.Interfaces.RedditRepository;

/// <summary>
/// Represents the interface for interacting with the data repositories for Reddit posts.
/// </summary>
public interface IPostRepository : IDisposable
{
    Task AddPostsAsync(IEnumerable<Link> records);

    Task<List<Link>> GetAllPostsAsync(string? subreddit, Pagination pagination);

    Task<IEnumerable<KeyValuePair<string, List<Link>>>> GetAllAuthorsAsync(string? subreddit, Pagination pagination);

    Task<long> GetPostsCountAsync(string? subreddit);

    Task<long> GetAuthorsCountAsync(string? subreddit);

    Task<IEnumerable<KeyValuePair<string, int>>> GetTopPosts(string? subreddit, int top = 10);

    Task<IEnumerable<KeyValuePair<string, int>>> GetTopAuthors(string? subreddit, int top = 10);
}
