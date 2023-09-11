namespace Neal.Reddit.Core.Entities.Exceptions;

[Serializable]
public class ConfigurationException<T> : Exception
{
	private static readonly string message = $"Missing {nameof(T)} configuration.";

    public ConfigurationException() : base(message) { }

	public ConfigurationException(Exception inner) : base(message, inner) { }
}
