using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Neal.Reddit.Client.Models;

public record Link
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("subreddit")]
    public string Subreddit { get; set; } = string.Empty;

    [JsonPropertyName("author_fullname")]
    public string AuthorId { get; set; } = string.Empty;

    [JsonPropertyName("author")]
    public string AuthorName { get; set; } = string.Empty;

    [JsonPropertyName("ups")]
    public int Ups { get; set; }

    [JsonPropertyName("downs")]
    public int Downs { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("upvote_ratio")]
    public double UpvoteRatio { get; set; }

    [JsonPropertyName("total_awards_received")]
    public int TotalAwards { get; set; }

    [JsonPropertyName("num_comments")]
    public int CommentCount { get; set; }

    [JsonPropertyName("is_video")]
    public bool IsVideo { get; set; }

    [JsonPropertyName("created_utc")]
    public double CreatedUtcEpoch { get; set; }
}
