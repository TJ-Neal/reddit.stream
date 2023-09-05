﻿namespace Neal.Reddit.Application.Constants.Messages;

public struct ApplicationStatusMessages
{
    public const string FatalError = "Application failed unexpectedly.";

    public const string HostedServiceFinished = "Hosted service finished.";

    public const string HostedServiceStarting = "Hosted service starting...";

    public const string Started = "Application has been started.";

    public const string Stopped = "Application has been stopped.";

    public const string RecordCount = "{source} running. Currently has {recordCount} records and {authors} author(s).";
}
