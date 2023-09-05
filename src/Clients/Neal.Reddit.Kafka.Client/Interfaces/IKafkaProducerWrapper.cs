using Neal.Reddit.Application.Interfaces;
using Neal.Reddit.Core.Entities.Kafka;

namespace Neal.Reddit.Client.Kafka.Interfaces;

/// <summary>
/// Represents a wrapper for events produced for the Kafka producer wrapper.
/// </summary>
public interface IKafkaProducerWrapper : IProducerWrapper<KafkaProducerMessage>
{
    void Flush();
}
