using Microsoft.Extensions.Logging;
using Neal.Reddit.Application.Constants.Messages;
using Neal.Reddit.Application.Utilities;
using Neal.Reddit.Client.Faster.Constants;
using Neal.Reddit.Client.Faster.Interfaces;
using Neal.Reddit.Core.Entities.Configuration;
using Neal.Reddit.Core.Entities.Reddit;
using System.Net.Http.Json;

namespace Neal.Reddit.Client.Faster.Wrappers;

/// <summary>
/// <inheritdoc cref="IFasterProducerWrapper" />
/// </summary>
public sealed class FasterProducerWrapper : IFasterProducerWrapper
{
    #region Fields

    private readonly FasterConfiguration configuration;

    private readonly ILogger<FasterProducerWrapper> logger;

    private readonly IHttpClientFactory clientFactory;

    private bool hasFaulted = false;

    #endregion Fields

    public FasterProducerWrapper(
        FasterConfiguration fasterConfiguration,
        ILogger<FasterProducerWrapper> logger,
        IHttpClientFactory clientFactory)
    {
        this.configuration = fasterConfiguration;
        this.logger = logger;
        this.clientFactory = clientFactory;
    }

    public async Task ProduceAsync(Link message, CancellationToken cancellationToken)
    {
        if (!this.configuration.Enabled
            || hasFaulted
            || cancellationToken.IsCancellationRequested)
        {
            return;
        }

        if (string.IsNullOrEmpty(this.configuration.BaseUrl))
        {
            throw new InvalidOperationException(CommonLogMessages.InvalidConfiguration);
        }

        int retries = 0;

        while (retries <= RetryUtilities.MaxRetries)
        {
            try
            {
                using var client = clientFactory.CreateClient();

                var result = await client.PostAsJsonAsync(this.configuration.BaseUrl, new List<Link> { message }, cancellationToken);

                if (result is null || !result.IsSuccessStatusCode)
                {
                    this.logger.LogError(HandlerLogMessages.ProducerError, message.Name, message);
                }
                else
                {
                    this.logger.LogDebug(HandlerLogMessages.PrintResult, DateTime.Now, result.StatusCode, message.Name);

                    return;
                }
            }
            catch (Exception ex)
            {
                this.logger.LogCritical(HandlerLogMessages.ProducerException, ex.Message);

                // Exit loop if exception is not transient
                if (!RetryUtilities.ExceptionIsTransient(ex))
                {
                    this.hasFaulted = true;

                    return;
                }
            }

            retries++;
            await Task.Delay(RetryUtilities.GetDelay(retries), cancellationToken);
        }

        this.hasFaulted = true;
    }
}