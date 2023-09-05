using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Neal.Reddit.Application.Constants.Messages;
using Neal.Reddit.Client.Kafka.Constants;
using Neal.Reddit.Client.Kafka.Interfaces;
using Neal.Reddit.Core.Entities.Configuration;
using Neal.Reddit.Core.Entities.Kafka;

namespace Neal.Reddit.Client.Kafka.Wrappers;

/// <summary>
/// Represents a wrapper for interacting with the Kafka producer client and Kafka production of message logs from a Kafka message broker.
/// </summary>
public class KafkaProducerWrapper : IKafkaProducerWrapper, IDisposable
{
    #region Fields

    private readonly ILogger<KafkaProducerWrapper> logger;

    private readonly IProducer<string, string>? producer;

    private int producedCount = 0;

    private readonly object producerLock = new();

    #endregion Fields

    public KafkaProducerWrapper(
        ILogger<KafkaProducerWrapper> logger,
        WrapperConfiguration<ProducerConfig> wrapperConfiguration)
    {
        this.logger = logger;

        if (wrapperConfiguration is null
            || string.IsNullOrEmpty(wrapperConfiguration?.ClientConfig?.BootstrapServers))
        {
            throw new KeyNotFoundException(string.Format(ExceptionMessages.RequiredKeyNotFound, nameof(wrapperConfiguration.ClientConfig.BootstrapServers)));
        }

        try
        {
            producer = new ProducerBuilder<string, string>(wrapperConfiguration.ClientConfig).Build();
        }
        catch (Exception ex)
        {
            this.logger.LogCritical(ExceptionMessages.InstantiationError, nameof(producer), ex);
        }
    }

    public Task ProduceAsync(KafkaProducerMessage message, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }

        try
        {
            void callback(DeliveryReport<string, string> deliveryReport)
            {
                if (deliveryReport.Error.IsFatal)
                {
                    throw new ProduceException<string, string>(deliveryReport.Error, deliveryReport);
                }
                else
                {
                    lock (producerLock)
                    {
                        producedCount++;

                        if (producedCount % 1000M == 0)
                        {
                            logger.LogInformation(
                                HandlerLogMessages.PrintProducerResult,
                                deliveryReport.Timestamp.UtcDateTime.ToLocalTime().ToShortTimeString(),
                                deliveryReport.Status,
                                deliveryReport.Topic,
                                producedCount);
                        }
                    }
                }
            }

            producer?
                .Produce(
                    message.Topic,
                    message.Message,
                    callback);
        }
        catch (Exception ex)
        {
            logger.LogCritical(
                HandlerLogMessages.ProducerException,
                message.Message.Key,
                message.Message.Value,
                message.Topic,
                ex.Message);
        }

        return Task.CompletedTask;
    }

    public void Flush()
    {
        producer?.Flush();
        logger.LogInformation(CommonLogMessages.Flushed, nameof(producer));
    }

    public void Dispose()
    {
        logger.LogInformation(CommonLogMessages.Disposing, nameof(producer));
        producer?.Dispose();

        GC.SuppressFinalize(this);
    }
}
