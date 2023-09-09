using Neal.Reddit.Core.Constants;
using Neal.Reddit.Core.Enums;

namespace Neal.Reddit.Core.Entities.Configuration;

public class SubredditConfiguration
{
    public string Name { get; set; } = string.Empty;

    public Sorts Sort { get; set; } = Sorts.New;

    public int PerRequestLimit { get; set; } = Defaults.RateLimit;

    public MonitorTypes MonitorType { get; set; } = MonitorTypes.None;
}
