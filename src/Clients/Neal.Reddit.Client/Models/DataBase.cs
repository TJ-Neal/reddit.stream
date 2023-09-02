using System.Text.Json.Serialization;

namespace Neal.Reddit.Client.Models;

public record DataBase
{
    [JsonPropertyName("subreddit_id")]
    public string SubredditId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int Ups { get; set; }

    public int Downs { get; set; }

    public int Score { get; set; }

    [JsonPropertyName("author")]
    public string AuthorName { get; set; } = string.Empty;

    [JsonPropertyName("author_fullname")]
    public string AuthorId { get; set; } = string.Empty;

    [JsonPropertyName("num_comments")]
    public int CommentCount { get; set; }

    [JsonPropertyName("created_utc")]
    public double CreatedUtcEpoch { get; set; }
}
