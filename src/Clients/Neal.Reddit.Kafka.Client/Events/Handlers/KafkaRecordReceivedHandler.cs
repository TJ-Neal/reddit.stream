using Confluent.Kafka;
using MediatR;
using Microsoft.Extensions.Logging;
using Neal.Reddit.Application.Events.Notifications;
using Neal.Reddit.Client.Kafka.Constants;
using Neal.Reddit.Client.Kafka.Interfaces;
using Neal.Reddit.Core.Entities.Configuration;
using Neal.Reddit.Core.Entities.Kafka;
using Neal.Reddit.Core.Entities.Reddit;

namespace Neal.Reddit.Client.Kafka.Events.Handlers;

/// <summary>
/// Implementation of an event handler for received tweets to capture them and publish them to Kafka
/// </summary>
public class KafkaRecordReceivedHandler : INotificationHandler<KafkaRecordReceivedNotification>
{
    #region Fields

    private readonly string topic;

    private readonly IKafkaProducerWrapper kafkaProducerWrapper;

    private readonly ILogger<KafkaRecordReceivedHandler> logger;

    #endregion Fields

    public KafkaRecordReceivedHandler(IKafkaProducerWrapper kafkaProducer, ILogger<KafkaRecordReceivedHandler> logger, WrapperConfiguration<ProducerConfig> wrapperConfiguration)
    {
        topic = string.IsNullOrEmpty(wrapperConfiguration.Topic)
                ? throw new KafkaException(ErrorCode.Local_UnknownTopic)
                : wrapperConfiguration.Topic;
        kafkaProducerWrapper = kafkaProducer;
        this.logger = logger;
    }

    #region INotificationHandler Implementation

    public void Dispose() => kafkaProducerWrapper.Flush();

    /// <summary>
    /// Handle when a <seealso cref="KafkaRecordReceivedNotification"/> notification is received, publishing to the Kafka stream.
    /// </summary>
    public async Task Handle(KafkaRecordReceivedNotification notification, CancellationToken cancellationToken)
    {
        if (notification is null)
        {
            logger.LogDebug(HandlerLogMessages.NullNotification);

            return;
        }

        if (notification.Record is null)
        {
            logger.LogDebug(HandlerLogMessages.NullRecord);

            return;
        }

        if (notification.Record.Name is null || string.IsNullOrWhiteSpace(notification.Record.Name))
        {
            logger.LogDebug(HandlerLogMessages.NullRecordId);

            return;
        }

        await kafkaProducerWrapper
            .ProduceAsync(
                new KafkaProducerMessage(
                    new Message<string, DataBase>
                    {
                        Key = notification.Record.Name.ToString()!,
                        Value = notification.Record
                    },
                    topic
                ),
                cancellationToken);
    }

    #endregion INotificationHandler Implementation
}
