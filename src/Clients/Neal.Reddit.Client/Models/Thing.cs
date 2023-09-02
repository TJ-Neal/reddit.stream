using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Neal.Reddit.Client.Models;

public record Thing<T> where T : class
{
    [JsonPropertyName("kind")]
    public Kind Kind { get; set; }

    [JsonPropertyName("data")]
    public T? Data { get; set; }
}
