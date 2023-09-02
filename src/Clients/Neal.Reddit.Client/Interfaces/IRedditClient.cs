﻿using Neal.Reddit.Client.Models;

namespace Neal.Reddit.Client.Interfaces;

public interface IRedditClient
{
    public Task<ApiResponse<Listing<Link>>> GetSubredditPostsNewAsync(
        string subredditId,
        string before = "",
        string after = "",
        string show = "all",
        int limit = 100);

    public Task<ApiResponse<Listing<Comment>>> GetSubredditCommentsNewAsync(
        string subredditId,
        string before = "",
        string after = "",
        string show = "all",
        int limit = 100);
}