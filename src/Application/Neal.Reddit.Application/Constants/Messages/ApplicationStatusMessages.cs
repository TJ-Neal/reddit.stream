namespace Neal.Reddit.Application.Constants.Messages;

/// <summary>
/// Represents strings for the various states of the status of the application.
/// </summary>
public struct ApplicationStatusMessages
{
    public const string FatalError = "Application failed unexpectedly.";

    public const string HostedServiceFinished = "Hosted service finished.";

    public const string HostedServiceStarting = "Hosted service starting...";

    public const string Started = "Application has been started.";

    public const string Stopped = "Application has been stopped.";

    public const string PostsCount = "{source} running. Currently has {postsCount} records and {authors} author(s).";
}
