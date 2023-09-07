using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Neal.Reddit.Application.Constants.Api;
using Neal.Reddit.Application.Interfaces.RedditRepository;
using Neal.Reddit.Core.Entities.Configuration;
using Neal.Reddit.Core.Entities.Reddit;

namespace Neal.Reddit.API.Kafka.Reddit.Endpoints;

/// <summary>
/// Represents endpoints for this API to interact with the underlying repository.
/// This repository uses the Kafka technologies to accomplish a message bus architecture while maintaining a performant, scalable, and durable design
/// </summary>
public static class RepositoryEndpoints
{
    public static void MapRepositoryEndpoints(this IEndpointRouteBuilder routes, string groupTag)
    {
        //var group = routes.MapGroup(ApiStrings.BaseRoute)
        //    .WithTags(groupTag);

        //group.MapGet(ApiStrings.TweetsRoute,
        //        ([FromServices] IRedditRepository model, [FromQuery] int? page, [FromQuery] int? pageSize) => model.GetAllRecordsAsync(new Pagination(page, pageSize)))
        //    .WithName(ApiStrings.Get)
        //    .WithDescription(ApiStrings.GetTweetsDescription)
        //    .WithOpenApi();

        //group.MapPost(ApiStrings.TweetsRoute, ([FromServices] IRedditRepository model, [FromBody] List<DataBase> tweets) => model.AddRecordsAsync(tweets))
        //    .WithName(ApiStrings.PostTweetsName)
        //    .WithDescription(ApiStrings.PostTweetsDescription);

        //group.MapGet(ApiStrings.CountRoute, ([FromServices] IRedditRepository model) => model.GetCountAsync())
        //    .WithName(ApiStrings.GetCountName)
        //    .WithDescription(ApiStrings.GetCountDescription)
        //    .WithOpenApi();

        //group.MapGet(ApiStrings.HashtagsRoute, ([FromServices] IRedditRepository model, [FromQuery] int? top) => model.GetTopAuthors(top ?? 10))
        //    .WithName(ApiStrings.GetHashtagsName)
        //    .WithDescription(ApiStrings.GetHashtagsDescription)
        //    .WithOpenApi();
    }
}