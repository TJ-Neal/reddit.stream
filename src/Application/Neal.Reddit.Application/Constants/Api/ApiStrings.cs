namespace Neal.Reddit.Application.Constants.Api;

/// <summary>
/// Represents strings used to define the Application API
/// </summary>
public struct ApiStrings
{
    public const string BaseRoute = "/api";

    public const string GetPostsRoute = "/posts";

    public const string GetPostsName = "GetAllPosts";

    public const string GetPostsDescription = "Get All post that have been received. * cached for 1 minute";

    public const string GetAuthorsRoute = "/authors";

    public const string GetAuthorsName = "GetAllAuthors";

    public const string GetAuthorsDescription = "Get all Authors that have been received. * cached for 1 minute";

    public const string AddPostsRoute = GetPostsRoute;

    public const string AddPostsName = "AddPosts";

    public const string AddPostsDescription = "Add list of additional posts to the repository.";

    public const string PostsCountRoute = "/posts/count";

    public const string PostsCountName = "GetPostsCount";

    public const string PostsCountDescription = "Get the current count of Posts received. * cached for 1 minute";

    public const string AuthorsCountRoute = "/authors/count";

    public const string AuthorsCountName = "GetAuthorsCount";

    public const string AuthorsCountDescription = "Get the current count of Authors received. * cached for 1 minute";

    public const string TopUpsRoute = "/posts/top/ups";

    public const string TopUpsName = "GetTopUps";

    public const string TopUpsDescription = "Get the top [n] number of Posts received based on number of upvotes. * cached for 1 minute";

    public const string LowestUpvoteRatioRoute = "/posts/lowest/upvote_ratio";

    public const string LowestUpvoteRatioName = "GetLowestUpvoteRatio";

    public const string LowestUpvoteRatioDescription = "Get the least approved [n] number of Posts received based on lowest post ratio. * cached for 1 minute";

    public const string TopAuthorsRoute = "/authors/top/posts";

    public const string TopAuthorsName = "GetTopAuthors";

    public const string TopAuthorsDescription = "Get the top [n] number of Authors received. * cached for 1 minute";

    public const string TopCommentsRoute = "/posts/top/comments";

    public const string TopCommentsName = "GetTopPostByComments";

    public const string TopCommentsDescription = "Get the top [n] number of Posts received based on the number of comments. * cached for 1 minute";
}
