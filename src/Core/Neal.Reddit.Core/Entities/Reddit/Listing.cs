using System.Text.Json.Serialization;

namespace Neal.Reddit.Core.Entities.Reddit;

public record Listing
{
    public string After { get; set; } = string.Empty;

    public string Before { get; set; } = string.Empty;

    [JsonPropertyName("dist")]
    public int Count { get; set; }

    public IEnumerable<DataContainer<Link>>? Children { get; set; }
}
