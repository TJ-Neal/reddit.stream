using Neal.Reddit.Core.Constants;
using Neal.Reddit.Core.Enums;

namespace Neal.Reddit.Core.Entities.Configuration;

public class SubredditConfiguration
{
    public string Name { get; set; } = string.Empty;

    public int PerRequestLimit { get; set; } = Defaults.PerRequestMaxPosts;

    public MonitorTypes MonitorType { get; set; } = MonitorTypes.None;

    public bool ShouldMonitor => 
        this.MonitorType is MonitorTypes.AfterStartOnly or MonitorTypes.All;

    public SubredditConfiguration() { }

    public SubredditConfiguration(string name) => 
        this.Name = name;

    public SubredditConfiguration(string name, MonitorTypes monitorType)
    {
        this.Name = name;
        this.MonitorType = monitorType;
    }
}
