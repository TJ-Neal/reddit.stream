using System.Text.Json.Serialization;

namespace Neal.Reddit.Client.Models;

public record TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Represented in Unix Epoch Seconds from time of request
    /// </summary>
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("scope")]
    public string Scope { get; set; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.Now;
}