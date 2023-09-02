using System.Text.Json.Serialization;

namespace Neal.Reddit.Client.Models;

public record Comment : DataBase
{
    [JsonPropertyName("parent_id")]
    public string ParentId { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    [JsonPropertyName("body_html")]
    public string BodyHtml { get; set; } = string.Empty;
}
