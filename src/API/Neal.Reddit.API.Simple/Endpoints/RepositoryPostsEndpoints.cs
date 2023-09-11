using Microsoft.AspNetCore.Mvc;
using Neal.Reddit.Application.Constants.Api;
using Neal.Reddit.Application.Interfaces.RedditRepository;
using Neal.Reddit.Core.Entities.Configuration;
using Neal.Reddit.Core.Entities.Reddit;

namespace Neal.Reddit.API.Simple.Endpoints;

/// <summary>
/// Represents endpoints for this API to interact with posts in the underlying repository.
/// This repository uses basic in-memory data structures to provide performant data structures while sacrificing durability and scalability
/// </summary>
public static class RepositoryPostsEndpoints
{
    public static void MapRepositoryPostsEndpoints(this IEndpointRouteBuilder routes, string groupTag)
    {
        var group = routes.MapGroup(ApiStrings.BaseRoute)
            .WithTags(groupTag);

        group.MapGet(
                ApiStrings.GetPostsRoute,
                ([FromServices] IPostRepository model, [FromQuery] string? subreddit, [FromQuery] int? page, [FromQuery] int? pageSize) =>
                     model.GetAllPostsAsync(subreddit, new Pagination(page, pageSize)))
            .WithName(ApiStrings.GetPostsName)
            .WithDescription(ApiStrings.GetPostsDescription)
            .WithOpenApi();

        group.MapGet(
                ApiStrings.PostsCountRoute,
                ([FromServices] IPostRepository model, [FromQuery] string? subreddit) => 
                    model.GetPostsCountAsync(subreddit))
            .WithName(ApiStrings.PostsCountName)
            .WithDescription(ApiStrings.PostsCountDescription)
            .WithOpenApi();

        group.MapGet(
                ApiStrings.TopUpsRoute,
                ([FromServices] IPostRepository model, [FromQuery] string? subreddit, [FromQuery] int? top) =>                 
                    model.GetTopPostsByUpvotesAsync(subreddit, top ?? 10))
            .WithName(ApiStrings.TopUpsName)
            .WithDescription(ApiStrings.TopUpsDescription)
            .WithOpenApi();

        group.MapGet(
                ApiStrings.TopCommentsRoute,
                ([FromServices] IPostRepository model, [FromQuery] string? subreddit, [FromQuery] int? top) =>
                    model.GetTopPostsByCommentsAsync(subreddit, top ?? 10))
            .WithName(ApiStrings.TopCommentsName)
            .WithDescription(ApiStrings.TopCommentsDescription)
            .WithOpenApi();

        group.MapGet(
                ApiStrings.LowestUpvoteRatioRoute,
                ([FromServices] IPostRepository model, [FromQuery] string? subreddit, [FromQuery] int? top) =>
                    model.GetLowestPostsByUpvoteRatioAsync(subreddit, top ?? 10))
            .WithName(ApiStrings.LowestUpvoteRatioName)
            .WithDescription(ApiStrings.LowestUpvoteRatioDescription)
            .WithOpenApi();

        group.MapPost(
                ApiStrings.AddPostsRoute, 
                ([FromServices] IPostRepository model, [FromBody] List<Link> posts) => 
                    model.AddOrUpdatePostsAsync(posts))
            .WithName(ApiStrings.AddPostsName)
            .WithDescription(ApiStrings.AddPostsDescription)
            .WithOpenApi();
    }
}