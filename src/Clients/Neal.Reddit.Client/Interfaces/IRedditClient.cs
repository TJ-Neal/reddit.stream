using Neal.Reddit.Client.Models;
using Neal.Reddit.Core.Entities.Configuration;
using Neal.Reddit.Core.Entities.Reddit;

namespace Neal.Reddit.Client.Interfaces;

/// <summary>
/// Represents a client for interacting with the Reddit API
/// </summary>
public interface IRedditClient
{
    /// <summary>
    /// Retrieve <see cref="Link"/> posts using the <paramref name="configuration"/> 
    /// and execute <paramref name="postHandler"/> for each new post.
    /// </summary>
    /// <param name="configuration"><see cref="SubredditConfiguration"/> used to control execution</param>
    /// <param name="postHandler"><see cref="Func"/> to use for new posts</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Completed <see cref="Task"/> when execution is complete or canceled</returns>
    public Task GetPostsAsync(
        SubredditConfiguration configuration,
        Func<Link, CancellationToken, Task> postHandler,
        CancellationToken cancellationToken);
}