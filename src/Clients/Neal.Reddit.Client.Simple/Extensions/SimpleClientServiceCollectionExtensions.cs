using Microsoft.Extensions.DependencyInjection;
using Neal.Reddit.Client.Simple.Interfaces;
using Neal.Reddit.Client.Simple.Wrappers;
using Neal.Reddit.Core.Entities.Configuration;

namespace Neal.Reddit.Client.Simple.Extensions;

/// <summary>
/// Add the required types for dependency injection when the Simple client and repository are enabled according to the provided <see cref="SimpleConfiguration"/>.
/// </summary>
public static class SimpleClientServiceCollectionExtensions
{
    public static IServiceCollection AddSimpleRepositoryHandlerIfEnabled(this IServiceCollection services, SimpleConfiguration simpleConfiguration)
    {
        if (!simpleConfiguration.Enabled)
        {
            return services;
        }

        services
            .AddHttpClient()
            .AddSingleton(simpleConfiguration)
            .AddSingleton(typeof(ISimpleProducerWrapper), typeof(SimpleProducerWrapper))
            .AddMediatR(config => config.RegisterServicesFromAssembly(typeof(SimpleProducerWrapper).Assembly));

        return services;
    }
}