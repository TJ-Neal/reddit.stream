using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Neal.Reddit.Core.Entities.Configuration;
using Neal.Reddit.Kafka.Client.Events.Handlers;
using Neal.Reddit.Kafka.Client.Interfaces;
using Neal.Reddit.Kafka.Client.Wrappers;

namespace Neal.Reddit.Kafka.Client.Extensions;

/// <summary>
/// Add the required types for dependency injection when the Kafka client and repository are enabled according to the provided <see cref="KafkaProducerWrapperConfiguration"/>.
/// </summary>
public static class KafkaClientServiceCollectionExtensions
{
    public static IServiceCollection AddKafkaHandlerIfEnabled(
        this IServiceCollection services,
        WrapperConfiguration<ProducerConfig> kafkaProducerConfiguration)
    {
        if (!kafkaProducerConfiguration.Enabled)
        {
            return services;
        }

        services.AddKafkaClient();

        services
            .AddSingleton(kafkaProducerConfiguration)
            .AddSingleton(typeof(IKafkaProducerWrapper), typeof(KafkaProducerWrapper))
            .AddMediatR(config => config.RegisterServicesFromAssembly(typeof(KafkaRecordReceivedHandler).Assembly));

        return services;
    }
}