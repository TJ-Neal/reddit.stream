using System.Text.Json.Serialization;

namespace Neal.Reddit.Core.Entities.Reddit;

public record Link
{

    [JsonPropertyName("author")]
    public string AuthorName { get; set; } = string.Empty;

    [JsonPropertyName("author_fullname")]
    public string AuthorId { get; set; } = string.Empty;

    [JsonPropertyName("num_comments")]
    public int CommentCount { get; set; }

    [JsonPropertyName("created_utc")]
    public double CreatedUtcEpoch { get; set; }

    public int Downs { get; set; }

    [JsonPropertyName("is_video")]
    public bool IsVideo { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Score { get; set; }

    public string Subreddit { get; set; } = string.Empty;

    [JsonPropertyName("subreddit_id")]
    public string SubredditId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("total_awards_received")]
    public int TotalAwards { get; set; }

    public int Ups { get; set; }

    [JsonPropertyName("upvote_ratio")]
    public double UpvoteRatio { get; set; }
}
