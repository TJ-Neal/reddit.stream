using System.Text.Json.Serialization;

namespace Neal.Reddit.Client.Models;

public record Link : DataBase
{
    public string Subreddit { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("upvote_ratio")]
    public double UpvoteRatio { get; set; }

    [JsonPropertyName("total_awards_received")]
    public int TotalAwards { get; set; }

    [JsonPropertyName("is_video")]
    public bool IsVideo { get; set; }
}
