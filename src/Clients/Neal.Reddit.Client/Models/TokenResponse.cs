using Newtonsoft.Json;

namespace Neal.Reddit.Client.Models;

public record TokenResponse
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Represented in Unix Epoch Seconds from time of request
    /// </summary>
    [JsonProperty("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonProperty("scope")]
    public string Scope { get; set; } = string.Empty;

    [JsonProperty("token_type")]
    public string TokenType { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.Now;
}