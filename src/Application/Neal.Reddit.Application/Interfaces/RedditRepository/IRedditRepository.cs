using Neal.Reddit.Core.Entities.Configuration;
using Neal.Reddit.Core.Entities.Reddit;

namespace Neal.Reddit.Application.Interfaces.RedditRepository;

public interface IRedditRepository : IDisposable
{
    Task AddRecordsAsync(IEnumerable<DataBase> records);

    Task<List<DataBase>> GetAllRecordsAsync(Pagination pagination);

    Task<long> GetCountAsync();

    Task<IEnumerable<KeyValuePair<string, int>>> GetTopAuthors(int top = 10);
}
