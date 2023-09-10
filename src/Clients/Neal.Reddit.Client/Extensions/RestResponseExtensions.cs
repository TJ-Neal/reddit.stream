using Neal.Reddit.Core.Constants;
using RestSharp;

namespace Neal.Reddit.Client.Extensions;

/// <summary>
/// Extends the <see cref="RestResponse"/> class to add additional methods to retrieve rate limiting headers.
/// </summary>
public static class RestResponseExtensions
{
    /// <summary>
    /// Parse <paramref name="response"/> for a rate limit remaining header.
    /// </summary>
    /// <param name="response"><see cref="RestResponse"/> to parse.</param>
    /// <returns><c>int</c> representing rate limit remaining, <c>0</c> if no header found</returns>
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

    /// <summary>
    /// Parse <paramref name="response"/> for a rate limit used header.
    /// </summary>
    /// <param name="response"><see cref="RestResponse"/> to parse.</param>
    /// <returns><c>int</c> representing rate limit used, <c>0</c> if no header found</returns>
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

    /// <summary>
    /// Parse <paramref name="response"/> for a rate limit reset header.
    /// </summary>
    /// <param name="response"><see cref="RestResponse"/> to parse.</param>
    /// <returns><c>int</c> representing rate limit reset, <c>0</c> if no header found</returns>
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
