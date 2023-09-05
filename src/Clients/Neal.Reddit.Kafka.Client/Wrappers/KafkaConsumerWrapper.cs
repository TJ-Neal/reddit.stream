using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Neal.Reddit.Application.Constants.Messages;
using Neal.Reddit.Application.Utilities;
using Neal.Reddit.Client.Kafka.Constants;
using Neal.Reddit.Client.Kafka.Interfaces;
using Neal.Reddit.Core.Entities.Configuration;
using Neal.Reddit.Core.Entities.Reddit;
using System.Net.Http.Json;
using System.Text.Json;

namespace Neal.Reddit.Client.Kafka.Wrappers;

/// <summary>
/// Represents a wrapper for interacting with the Kafka consumer client and Kafka consuming message logs from a Kafka message broker.
/// </summary>
public class KafkaConsumerWrapper : IKafkaConsumerWrapper<string, string>
{
    private static bool hasFaulted = false;

    private readonly IConsumer<string, string> consumer;

    private readonly ILogger<KafkaConsumerWrapper> logger;

    private readonly IHttpClientFactory clientFactory;

    private readonly WrapperConfiguration<ConsumerConfig> configuration;

    public KafkaConsumerWrapper(WrapperConfiguration<ConsumerConfig> wrapperConfiguration, ILogger<KafkaConsumerWrapper> logger, IHttpClientFactory httpClientFactory)
    {
        this.logger = logger;
        clientFactory = httpClientFactory;
        configuration = wrapperConfiguration;

        if (configuration is null
            || string.IsNullOrEmpty(configuration?.ClientConfig?.BootstrapServers))
        {
            throw new KeyNotFoundException($"Key {nameof(configuration.ClientConfig.BootstrapServers)} was not found and is required.");
        }

        configuration.ClientConfig.GroupId = new Guid().ToString(); // Create a random consumer group so messages can be replayed from offset without special configuration
        consumer =
            new ConsumerBuilder<string, string>(configuration.ClientConfig)
            .Build();
        consumer.Subscribe(configuration.Topic);
        this.logger.LogInformation(WrapperLogMessages.Subscribed, wrapperConfiguration.Topic);
    }

    public async Task ConsumeAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(configuration.BaseUrl))
        {
            throw new InvalidOperationException(CommonLogMessages.InvalidConfiguration);
        }

        // TODO: Create a timer for resetting faulted so producers can retry after t time
        while (configuration.Enabled
            && !hasFaulted
            && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                var data = await GetNextSubscriptionResult(cancellationToken);

                if (!hasFaulted && data is not null)
                {
                    int retries = 0;

                    while (retries < RetryUtilities.MaxRetries)
                    {
                        try
                        {
                            using var client = clientFactory.CreateClient();

                            var postResult = await client.PostAsJsonAsync(configuration.BaseUrl, new List<DataBase> { data }, cancellationToken);

                            if (postResult is null || !postResult.IsSuccessStatusCode)
                            {
                                logger.LogError(HandlerLogMessages.ConsumerPostError, postResult?.StatusCode, postResult?.Content);
                            }
                            else
                            {
                                logger.LogDebug(HandlerLogMessages.PrintConsumeResult, DateTime.Now, postResult.StatusCode, data.Name);
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogCritical(HandlerLogMessages.ProducerException, ex.Message);

                            // Exit loop if exception is not transient
                            if (!RetryUtilities.ExceptionIsTransient(ex))
                            {
                                hasFaulted = true;
                                break;
                            }
                        }

                        await Task.Delay(RetryUtilities.GetDelay(++retries), cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogCritical(HandlerLogMessages.ConsumerException, ex.Message);
            }
        }
    }

    public void Dispose()
    {
        logger.LogInformation(CommonLogMessages.Disposing, nameof(KafkaConsumerWrapper));
        logger.LogInformation(CommonLogMessages.Disposing, nameof(consumer));

        try
        {
            if (consumer is not null)
            {
                consumer.Unsubscribe();
                consumer.Close();
                consumer.Dispose();
            }
        }
        catch (Exception ex)
        {
            logger
                .LogError(ExceptionMessages.DisposeException, nameof(consumer), ex.Message);
        }

        GC.SuppressFinalize(this);
    }

    private async Task<DataBase?> GetNextSubscriptionResult(CancellationToken cancellationToken)
    {
        try
        {
            int retries = 0;

            while (retries < RetryUtilities.MaxRetries)
            {
                var consumeResult = await Task.Run(() => consumer.Consume(cancellationToken));

                if (consumeResult is not null)
                {
                    var data = JsonSerializer.Deserialize<DataBase>(consumeResult.Message.Value);

                    if (data is not null)
                    {
                        logger.LogDebug(
                            HandlerLogMessages.PrintConsumeResult,
                            DateTime.Now.ToUniversalTime(),
                            consumeResult.Message.Key,
                            consumeResult.Message.Timestamp.UtcDateTime);

                        return data;
                    }
                }

                await Task.Delay(RetryUtilities.GetDelay(++retries), cancellationToken);
            }

            hasFaulted = retries >= RetryUtilities.MaxRetries;
        }
        catch (Exception ex)
        {
            logger.LogCritical(HandlerLogMessages.ConsumerException, ex.Message);
        }

        return default;
    }
}
