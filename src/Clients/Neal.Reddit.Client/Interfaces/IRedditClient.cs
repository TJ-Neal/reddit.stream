using Neal.Reddit.Client.Models;
using Neal.Reddit.Core.Entities.Reddit;

namespace Neal.Reddit.Client.Interfaces;

public interface IRedditClient
{
    public Task<ApiResponse> GetPostsNewAsync(
        SubredditConfiguration configuration,
        string before = "",
        string after = "",
        string show = "all",
        int limit = 100);

    public Task MonitorPostsAsync(
        SubredditConfiguration configuration,
        Func<Link, Task> newPostHandler, 
        CancellationToken cancellationToken);
}