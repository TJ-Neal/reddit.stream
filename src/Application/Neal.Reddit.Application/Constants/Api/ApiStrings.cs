namespace Neal.Reddit.Application.Constants.Api;

/// <summary>
/// Represents strings used to define the Application API
/// </summary>
public struct ApiStrings
{
    public const string BaseRoute = "/api";

    public const string GetPostsRoute = "/posts";

    public const string GetPostsName = "GetAllPosts";

    public const string GetPostsDescription = "Get All post that have been received.";

    public const string GetAuthorsRoute = "/authors";

    public const string GetAuthorsName = "GetAllAuthors";

    public const string GetAuthorsDescription = "Get all Authors that have been received.";

    public const string AddPostsRoute = GetPostsRoute;

    public const string AddPostsName = "AddPosts";

    public const string AddPostsDescription = "Add list of additional posts to the repository.";

    public const string PostsCountRoute = "/posts/count";

    public const string PostsCountName = "GetPostsCount";

    public const string PostsCountDescription = "Get the current count of Posts received.";

    public const string AuthorsCountRoute = "/authors/count";

    public const string AuthorsCountName = "GetAuthorsCount";

    public const string AuthorsCountDescription = "Get the current count of Authors received.";

    public const string TopPostsRoute = "/posts/top";

    public const string TopPostsName = "GetTopPosts";

    public const string TopPostsDescription = "Get the top n number of Posts received.";

    public const string TopAuthorsRoute = "/authors/top";

    public const string TopAuthorsName = "GetTopAuthors";

    public const string TopAuthorsDescription = "Get the top n number of Authors received.";
}
