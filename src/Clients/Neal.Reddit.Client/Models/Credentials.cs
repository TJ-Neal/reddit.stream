namespace Neal.Reddit.Client.Models;

public record Credentials
{
    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public string DeviceId { get; set; } = string.Empty;
}
