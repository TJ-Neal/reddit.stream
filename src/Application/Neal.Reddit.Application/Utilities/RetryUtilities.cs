using System.Net;

namespace Neal.Reddit.Application.Utilities;

/// <summary>
/// Utility to provide retry functionality to web requests with exponential back-off and maximum retries.
/// </summary>
public static class RetryUtilities
{
    private const int EXPONENT = 2;

    private const int DELAY = 1;

    /// <summary>
    /// Maximum number of times to retry.
    /// </summary>
    public static int MaxRetries { get; } = 10;

    /// <summary>
    /// Whether the <paramref name="exception"/> is recoverable.
    /// </summary>
    /// <param name="exception"><see cref="Exception"/> encountered.</param>
    /// <returns><c>true</c> if exception can be recovered from.</returns>
    public static bool ExceptionIsTransient(Exception exception)
    {
        if (exception is WebException webException)
        {
            switch (webException.Status)
            {
                case WebExceptionStatus.ConnectionClosed:
                case WebExceptionStatus.Timeout:
                case WebExceptionStatus.RequestCanceled:
                case WebExceptionStatus.UnknownError:
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Calculate the amount of delay that should be used for the next retry.
    /// </summary>
    /// <param name="retries">How many retries have been tried.</param>
    /// <returns><see cref="TimeSpan"/> of delay before next retry.</returns>
    public static TimeSpan GetDelay(int retries) => TimeSpan.FromSeconds(retries * EXPONENT * DELAY);
}
