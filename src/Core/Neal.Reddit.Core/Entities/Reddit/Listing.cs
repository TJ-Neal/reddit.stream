using System.Text.Json.Serialization;

namespace Neal.Reddit.Core.Entities.Reddit;

public record Listing<T> where T : Link
{
    public string After { get; set; } = string.Empty;

    public string Before { get; set; } = string.Empty;

    [JsonPropertyName("dist")]
    public int Count { get; set; }

    public IEnumerable<DataContainer<T>>? Children { get; set; }
}
