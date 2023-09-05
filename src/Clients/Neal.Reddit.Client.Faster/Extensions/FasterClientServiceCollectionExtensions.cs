using Microsoft.Extensions.DependencyInjection;
using Neal.Reddit.Client.Faster.Events.Handlers;
using Neal.Reddit.Client.Faster.Interfaces;
using Neal.Reddit.Client.Faster.Wrappers;
using Neal.Reddit.Core.Entities.Configuration;

namespace Neal.Reddit.Client.Faster.Extensions;

/// <summary>
/// Add the required types for dependency injection when the FasterKV client and repository are enabled according to the provided <see cref="FasterConfiguration"/>.
/// </summary>
public static class FasterClientServiceCollectionExtensions
{
    public static IServiceCollection AddFasterRepositoryHandlerIfEnabled(this IServiceCollection services, FasterConfiguration fasterConfiguration)
    {
        if (!fasterConfiguration.Enabled)
        {
            return services;
        }

        services
            .AddHttpClient()
            .AddSingleton(fasterConfiguration)
            .AddSingleton(typeof(IFasterProducerWrapper), typeof(FasterProducerWrapper))
            .AddMediatR(config => config.RegisterServicesFromAssembly(typeof(FasterRecordReceivedHandler).Assembly));

        return services;
    }
}