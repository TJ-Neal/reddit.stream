namespace Neal.Reddit.Application.Constants.Messages;

/// <summary>
/// Represents strings for the common log messages used in the application.
/// </summary>
public struct CommonLogMessages
{
    public const string CanceledException = "Task canceled with exception.";

    public const string CancelRequested = "Cancellation requested.";

    public const string InvalidConfiguration = "Configuration is not valid. Missing base url.";

    public const string Disposing = "Disposing {object}.";

    public const string StartingLoop = "Starting background loop...";

    public const string Flushed = "{name} is being flushed.";

    public const string HttpRequestError = "Error executing http request {statusCode} - {statusDescription}";

    public const string HttpRequestException = "Exception executing http request\n\t{exception";

    public const string TaskDelay = "Delaying {wait} seconds as {reason} back-off.";

    public const string InstantiationError = "Exception creating {object}\n\t{exception}";
}