using System.Text.Json.Serialization;

namespace Neal.Reddit.Client.Models;

public record ApiListingResponse
{
    [JsonPropertyName("kind")]
    public Kind Kind { get; set; }

    [JsonPropertyName("data")]
    public Listing Data { get; set; } = new Listing();
}
