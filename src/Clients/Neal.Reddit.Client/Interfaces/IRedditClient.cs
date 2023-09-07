using Neal.Reddit.Client.Models;
using Neal.Reddit.Core.Entities.Reddit;

namespace Neal.Reddit.Client.Interfaces;

public interface IRedditClient
{
    public Task<ApiResponse<Listing<Link>>> GetSubredditPostsNewAsync(
        string subredditId,
        string before = "",
        string after = "",
        string show = "all",
        int limit = 100);
}