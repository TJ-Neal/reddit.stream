using Neal.Reddit.Core.Constants;
using Neal.Reddit.Core.Enums;

namespace Neal.Reddit.Core.Entities.Configuration;

public class SubredditConfiguration
{
    public string Name { get; set; } = string.Empty;

    public Sorts Sort { get; set; } = Sorts.New;

    public int PerRequestLimit { get; set; } = Defaults.RateLimit;

    public MonitorTypes MonitorType { get; set; } = MonitorTypes.None;

    public bool ShouldMonitor
    {
        get => this.MonitorType
            is MonitorTypes.AfterStartOnly or MonitorTypes.All;
    }

    public SubredditConfiguration() { }

    public SubredditConfiguration(string name) => 
        this.Name = name;

    public SubredditConfiguration(string name, MonitorTypes monitorType)
    {
        this.Name = name;
        this.MonitorType = monitorType;
    }
}
