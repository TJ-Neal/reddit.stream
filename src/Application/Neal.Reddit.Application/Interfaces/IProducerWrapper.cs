namespace Neal.Reddit.Application.Interfaces;

/// <summary>
/// Represents the wrapper functionality for an event producer using a <typeparamref name="TMessage"/> message to produce an event.
/// </summary>
/// <typeparam name="TMessage"></typeparam>
public interface IProducerWrapper<TMessage> where TMessage : class
{
    /// <summary>
    /// Produce a <typeparamref name="TMessage"/> onto the message bus
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    abstract Task ProduceAsync(TMessage message, CancellationToken cancellationToken);
}