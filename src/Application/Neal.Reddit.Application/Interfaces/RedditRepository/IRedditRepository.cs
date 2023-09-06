using Neal.Reddit.Core.Entities.Configuration;
using Neal.Reddit.Core.Entities.Reddit;

namespace Neal.Reddit.Application.Interfaces.RedditRepository;

public interface IRedditRepository : IDisposable
{
    Task AddPostsAsync(IEnumerable<Link> records);

    Task<List<Link>> GetAllPostsAsync(Pagination pagination);

    Task<List<string>> GetAllAuthorsAsync(Pagination pagination);

    Task<long> GetCountAsync();

    Task<IEnumerable<KeyValuePair<string, int>>> GetTopPosts(int top = 10);

    Task<IEnumerable<KeyValuePair<string, int>>> GetTopAuthors(int top = 10);
}
