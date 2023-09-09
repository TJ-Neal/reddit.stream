using Neal.Reddit.Client.Models;
using Neal.Reddit.Core.Entities.Configuration;
using Neal.Reddit.Core.Entities.Reddit;

namespace Neal.Reddit.Client.Interfaces;

public interface IRedditClient
{
    public Task GetPostsAsync(
        SubredditConfiguration configuration,
        Func<Link, Task> postHandler,
        CancellationToken cancellationToken);
}