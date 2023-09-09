using Neal.Reddit.Core.Constants;
using Neal.Reddit.Core.Entities.Configuration;

namespace Neal.Reddit.Core.Entities.Reddit;

public class RedditPostRequest : SubredditConfiguration
{
    public string Show { get; init; }

    public Func<Link, Task> PostHandler { get; init; }

    public RedditPostRequest(
        SubredditConfiguration configuration,
        Func<Link, Task> postHandler,
        string show = ParameterStrings.All)
    {
        MonitorType = configuration.MonitorType;
        Name = configuration.Name;
        PerRequestLimit = configuration.PerRequestLimit;
        PostHandler = postHandler;
        Show = show;
        Sort = configuration.Sort;
    }
}
