using Neal.Reddit.Core.Entities.Reddit;

namespace Neal.Reddit.Client.Models;

public record ApiResponse
{
    public double RateLimitRemaining { get; init; }

    public int RateLimitUsed { get; init; }

    public int RateLimitReset { get; init; }

    public DataContainer<Listing>? Root { get; init; }
}
