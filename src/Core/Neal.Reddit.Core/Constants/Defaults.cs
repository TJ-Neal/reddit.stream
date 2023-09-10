namespace Neal.Reddit.Core.Constants;

public struct Defaults
{
    public const int PerRequestMaxPosts = 100;

    public const int MaxRequestsPerReset = 600;

    public const int ResetSeconds = 600;

    public const double ReteLimitDelay = 0.1; // Buffer

    public const int LowUpdateDelaySeconds = 15;
}
