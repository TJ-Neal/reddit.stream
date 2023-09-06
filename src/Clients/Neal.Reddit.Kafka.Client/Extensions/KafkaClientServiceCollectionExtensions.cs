using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Neal.Reddit.Client.Kafka.Events.Handlers;
using Neal.Reddit.Client.Kafka.Interfaces;
using Neal.Reddit.Client.Kafka.Wrappers;
using Neal.Reddit.Core.Entities.Configuration;

namespace Neal.Reddit.Client.Kafka.Extensions;

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
            .AddMediatR(config => config.RegisterServicesFromAssembly(typeof(KafkaPostReceivedHandler).Assembly));

        return services;
    }
}