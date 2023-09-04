using Confluent.Kafka;
using Neal.Reddit.Core.Entities.Reddit;
using System.Text.Json;

namespace Neal.Reddit.Core.Entities.Kafka;

public class KafkaProducerMessage
{
    public Message<string, string> Message { get; init; }

    public string Topic { get; init; }

    public KafkaProducerMessage(Message<string, DataBase> message, string topic)
    {
        this.Message = new Message<string, string>
        {
            Key = message.Key,
            Value = JsonSerializer.Serialize(message.Value)
        };
        this.Topic = topic;
    }
}