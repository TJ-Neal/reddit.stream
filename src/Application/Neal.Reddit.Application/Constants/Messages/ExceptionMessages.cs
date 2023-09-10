namespace Neal.Reddit.Application.Constants.Messages;

/// <summary>
/// Represents various strings for used in exception messages used within the application.
/// </summary>
public struct ExceptionMessages
{
    public const string DisposeException = "There was an exception while disposing [{0}]\n{1}";

    public const string ErrorDuringLoop = "Failed during loop executions\n\t{error}";

    public const string GenericException = "Exception encountered during execution - {0}";

    public const string KeyNotFound = "Unable to read key.";

    public const string RequiredKeyNotFound = "Key {0} was not found and is required.";

    public const string SerializationError = "{0} is not valid cannot be serialized.";

    public const string HttpRequestError = "Error executing http request {0} - {1}";
}