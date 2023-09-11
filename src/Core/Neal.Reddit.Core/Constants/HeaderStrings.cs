namespace Neal.Reddit.Core.Constants;

public struct HeaderStrings
{
    public const string GrantTypeKey = "grant_type";

    public const string GrantTypeValue = "client_credentials";

    public const string DeviceIdKey = "device_id";

    public const string LimitRemaining = "x-ratelimit-remaining";

    public const string LimitUsed = "x-ratelimit-used";

    public const string LimitReset = "x-ratelimit-reset";
}