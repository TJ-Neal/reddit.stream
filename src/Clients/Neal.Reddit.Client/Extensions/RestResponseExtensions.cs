using Neal.Reddit.Core.Constants;
using RestSharp;

namespace Neal.Reddit.Client.Extensions;

public static class RestResponseExtensions
{
    public static int ParseRateLimitRemaining(this RestResponse response)
    {
        var rateLimitRemainingHeader = response
            .Headers
            ?.Where(x => x.Name == HeaderStrings.LimitRemaining)
            .Select(x => x.Value)
            .FirstOrDefault();

        _ = double.TryParse(rateLimitRemainingHeader?.ToString(), out var limitRemaining);

        return Convert.ToInt32(limitRemaining);
    }

    public static int ParseRateLimitUsed(this RestResponse response)
    {
        var rateLimitUsedHeader = response
            .Headers
            ?.Where(x => x.Name == HeaderStrings.LimitUsed)
            .Select(x => x.Value)
            .FirstOrDefault();


        _ = int.TryParse(rateLimitUsedHeader?.ToString(), out var limitUsed);

        return limitUsed;
    }

    public static int ParseRateLimitReset(this RestResponse response)
    {
        var rateLimitResetHeader = response
            .Headers
            ?.Where(x => x.Name == HeaderStrings.LimitReset)
            .Select(x => x.Value)
            .FirstOrDefault();

        _ = int.TryParse(rateLimitResetHeader?.ToString(), out var limitReset);

        return limitReset;
    }
}
