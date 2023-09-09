namespace Neal.Reddit.Client.Models;

public class SubredditConfiguration
{
    public string Name { get; set; } = string.Empty;

    public bool AfterStartOnly { get; set; } = false;

    public int PerRequestLimit { get; set; } = 100;
}
