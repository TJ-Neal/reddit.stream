using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Neal.Reddit.Client.Models;

public record Listing
{
    [JsonPropertyName("after")]
    public string After { get; set; } = string.Empty;

    [JsonPropertyName("before")]
    public string Before { get; set; } = string.Empty;

    [JsonPropertyName("dist")]
    public int Count { get; set; }

    [JsonPropertyName("children")]
    public IEnumerable<Thing<Link>> Children { get; set; } = Enumerable.Empty<Thing<Link>>();
}
