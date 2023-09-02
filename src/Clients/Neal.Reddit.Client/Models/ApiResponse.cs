namespace Neal.Reddit.Client.Models;

public record ApiResponse<T> where T : class
{
    public double RateLimitRemaining { get; init; }

    public int RateLimitUsed { get; init; }

    public int RateLimitReset { get; init; }

    public DataContainer<T>? Root { get; init; }
}
