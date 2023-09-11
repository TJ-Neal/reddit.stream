using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Neal.Reddit.Application.Constants.Api;
using Neal.Reddit.Application.Interfaces.RedditRepository;
using Neal.Reddit.Core.Entities.Configuration;
using Neal.Reddit.Core.Entities.Reddit;

namespace Neal.Reddit.API.Simple.Endpoints;

/// <summary>
/// Represents endpoints for this API to interact with the underlying repository.
/// This repository uses basic in-memory data structures to provide performant data structures while sacrificing durability and scalability
/// </summary>
public static class RepositoryEndpoints
{
    public static void MapRepositoryEndpoints(this IEndpointRouteBuilder routes, string groupTag)
    {
        var group = routes.MapGroup(ApiStrings.BaseRoute)
            .WithTags(groupTag);

        group.MapGet(
                ApiStrings.GetPostsRoute,
                (
                    [FromServices] IPostRepository model, 
                    [FromQuery] int? page, 
                    [FromQuery] int? pageSize) => model.GetAllPostsAsync(new Pagination(page, pageSize)))
            .WithName(ApiStrings.GetPostsName)
            .WithDescription(ApiStrings.GetPostsDescription)
            .WithOpenApi();

        group.MapGet(
                ApiStrings.GetAuthorsRoute,
                (
                    [FromServices] IPostRepository model,
                    [FromQuery] int? page,
                    [FromQuery] int? pageSize) => model.GetAllAuthorsAsync(new Pagination(page, pageSize)))
            .WithName(ApiStrings.GetAuthorsName)
            .WithDescription(ApiStrings.GetAuthorsDescription)
            .WithOpenApi();

        group.MapGet(
                ApiStrings.PostsCountRoute,
                ([FromServices] IPostRepository model) => model.GetPostsCountAsync())
            .WithName(ApiStrings.PostsCountName)
            .WithDescription(ApiStrings.PostsCountDescription)
            .WithOpenApi();

        group.MapGet(
                ApiStrings.AuthorsCountRoute,
                ([FromServices] IPostRepository model) => model.GetPostsCountAsync())
            .WithName(ApiStrings.AuthorsCountName)
            .WithDescription(ApiStrings.AuthorsCountDescription)
            .WithOpenApi();

        group.MapGet(
                ApiStrings.TopPostsRoute,
                ([FromServices] IPostRepository model, [FromQuery] int? top) => model.GetTopPosts(top ?? 10))
            .WithName(ApiStrings.TopPostsName)
            .WithDescription(ApiStrings.TopPostsDescription)
            .WithOpenApi();

        group.MapGet(
                ApiStrings.TopAuthorsRoute,
                ([FromServices] IPostRepository model, [FromQuery] int? top) => model.GetTopAuthors(top ?? 10))
            .WithName(ApiStrings.TopAuthorsName)
            .WithDescription(ApiStrings.TopAuthorsDescription)
            .WithOpenApi();

        group.MapPost(
            ApiStrings.AddPostsRoute, 
            (
                [FromServices] IPostRepository model,
                [FromBody] List<Link> posts) => model.AddPostsAsync(posts))
            .WithName(ApiStrings.AddPostsName)
            .WithDescription(ApiStrings.AddPostsDescription)
            .WithOpenApi();
    }
}