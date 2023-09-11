using Neal.Reddit.Core.Entities.Configuration;
using Neal.Reddit.Core.Entities.Reddit;

namespace Neal.Reddit.Application.Interfaces.RedditRepository;

/// <summary>
/// Represents the interface for interacting with the data repositories for Reddit posts.
/// </summary>
public interface IPostRepository : IDisposable
{
    /// <summary>
    /// Add the list of posts to the repository
    /// </summary>
    /// <param name="posts"><see cref="IEnumerable{T}"/> of posts to process</param>
    Task AddOrUpdatePostsAsync(IEnumerable<Link> posts);

    /// <summary>
    /// Get a paginated list of all posts
    /// </summary>
    /// <param name="subreddit">Optional filter for subreddit specific posts</param>
    /// <param name="pagination"><see cref="Pagination"/> for the request</param>
    /// <returns><see cref="IEnumerable{T}"/> of posts</returns>
    Task<IEnumerable<Link>> GetAllPostsAsync(string? subreddit, Pagination pagination);

    /// <summary>
    /// Get a paginated list of all authors
    /// </summary>
    /// <param name="subreddit">Optional filter for subreddit specific posts</param>
    /// <param name="pagination"><see cref="Pagination"/> for the request</param>
    /// <returns><see cref="IEnumerable{T}"/> of authors</returns>
    Task<IEnumerable<Author>> GetAllAuthorsAsync(string? subreddit, Pagination pagination);

    /// <summary>
    /// Get the total number of posts.
    /// </summary>
    /// <param name="subreddit">Optional filter for subreddit specific posts</param>    
    /// <returns>Number of posts</returns>
    Task<long> GetPostsCountAsync(string? subreddit);

    /// <summary>
    /// Get the total number of authors.
    /// </summary>
    /// <param name="subreddit">Optional filter for subreddit specific posts</param>    
    /// <returns>Number of authors</returns>
    Task<long> GetAuthorsCountAsync(string? subreddit);

    /// <summary>
    /// Get a list of <paramref name="top"/> posts by descending upvote count.
    /// </summary>
    /// <param name="subreddit">Optional filter for subreddit specific posts</param>
    /// <param name="top">Optional number of top posts to return, default is 10</param>
    /// <returns><see cref="IEnumerable{T}"/> of posts by upvote order.</returns>
    Task<IEnumerable<Link>> GetTopPostsByUpvotesAsync(string? subreddit, int top = 10);

    /// <summary>
    /// Get a list of <paramref name="top"/> posts by number of comments.
    /// </summary>
    /// <param name="subreddit">Optional filter for subreddit specific posts</param>
    /// <param name="top">Optional number of top posts to return, default is 10</param>
    /// <returns><see cref="IEnumerable{T}"/> of posts by number of comments order.</returns>
    Task<IEnumerable<Link>> GetTopPostsByCommentsAsync(string? subreddit, int top = 10);

    /// <summary>
    /// Get a list of <paramref name="top"/> posts with scores by ascending upvote ratio.
    /// </summary>
    /// <param name="subreddit">Optional filter for subreddit specific posts</param>
    /// <param name="top">Optional number of top posts to return, default is 10</param>
    /// <returns><see cref="IEnumerable{T}"/> of posts by ascending upvote ratio order.</returns>
    Task<IEnumerable<Link>> GetLowestPostsByUpvoteRatioAsync(string? subreddit, int top = 10);

    /// <summary>
    /// Get a list of <paramref name="top"/> authors by descending number of posts.
    /// </summary>
    /// <param name="subreddit">Optional filter for subreddit specific posts</param>
    /// <param name="top">Optional number of top posts to return, default is 10</param>
    /// <returns><see cref="IEnumerable{T}"/> of authors by descending number of posts order.</returns>
    Task<IEnumerable<Author>> GetTopAuthors(string? subreddit, int top = 10);
}
