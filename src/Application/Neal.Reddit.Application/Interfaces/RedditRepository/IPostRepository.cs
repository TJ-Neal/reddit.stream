using Neal.Reddit.Core.Entities.Configuration;
using Neal.Reddit.Core.Entities.Reddit;

namespace Neal.Reddit.Application.Interfaces.RedditRepository;

/// <summary>
/// Represents the interface for interacting with the data repositories for Reddit posts.
/// </summary>
public interface IPostRepository : IDisposable
{
    Task AddPostsAsync(IEnumerable<Link> records);

    Task<List<Link>> GetAllPostsAsync(Pagination pagination);

    Task<List<KeyValuePair<string, int>>> GetAllAuthorsAsync(Pagination pagination);

    Task<long> GetPostsCountAsync();

    Task<long> GetAuthorsCountAsync();

    Task<IEnumerable<KeyValuePair<string, int>>> GetTopPosts(int top = 10);

    Task<IEnumerable<KeyValuePair<string, int>>> GetTopAuthors(int top = 10);
}
