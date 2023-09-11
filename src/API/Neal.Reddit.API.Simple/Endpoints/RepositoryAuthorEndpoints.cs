using Microsoft.AspNetCore.Mvc;
using Neal.Reddit.Application.Constants.Api;
using Neal.Reddit.Application.Interfaces.RedditRepository;
using Neal.Reddit.Core.Entities.Configuration;

namespace Neal.Reddit.API.Simple.Endpoints;

/// <summary>
/// Represents endpoints for this API to interact with authors in the underlying repository.
/// This repository uses basic in-memory data structures to provide performant data structures while sacrificing durability and scalability
/// </summary>
public static class RepositoryAuthorEndpoints
{
    public static void MapRepositoryAuthorEndpoints(this IEndpointRouteBuilder routes, string groupTag)
    {
        var group = routes.MapGroup(ApiStrings.BaseRoute)
            .WithTags(groupTag);

        group.MapGet(
                ApiStrings.GetAuthorsRoute,
                ([FromServices] IPostRepository model, [FromQuery] string? subreddit, [FromQuery] int? page, [FromQuery] int? pageSize) =>
                    model.GetAllAuthorsAsync(subreddit, new Pagination(page, pageSize)))
            .WithName(ApiStrings.GetAuthorsName)
            .WithDescription(ApiStrings.GetAuthorsDescription)
            .WithOpenApi();

        group.MapGet(
                ApiStrings.AuthorsCountRoute,
                ([FromServices] IPostRepository model, [FromQuery] string? subreddit) =>
                    model.GetAuthorsCountAsync(subreddit))
            .WithName(ApiStrings.AuthorsCountName)
            .WithDescription(ApiStrings.AuthorsCountDescription)
            .WithOpenApi();

        group.MapGet(
                ApiStrings.TopAuthorsRoute,
                ([FromServices] IPostRepository model, [FromQuery] string? subreddit, [FromQuery] int? top) =>
                    model.GetTopAuthors(subreddit, top ?? 10))
            .WithName(ApiStrings.TopAuthorsName)
            .WithDescription(ApiStrings.TopAuthorsDescription)
            .WithOpenApi();
    }
}
