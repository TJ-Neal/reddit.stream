using Neal.Reddit.Core.Constants;
using Neal.Reddit.Core.Entities.Configuration;
using Neal.Reddit.Core.Enums;

namespace Neal.Reddit.Core.Entities.Reddit;

public class RedditPostRequest : SubredditConfiguration
{
    public string Show { get; init; }

    public Func<Link, CancellationToken, Task> PostHandler { get; init; }

    public RedditPostRequest(
        SubredditConfiguration configuration,
        Func<Link, CancellationToken, Task> postHandler,
        string show = ParameterStrings.All)
    {
        this.MonitorType = configuration.MonitorType;
        this.Name = configuration.Name;
        this.PerRequestLimit = configuration.PerRequestLimit;
        this.PostHandler = postHandler;
        this.Show = show;
    }
}
