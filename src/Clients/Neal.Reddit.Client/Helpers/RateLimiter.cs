using Neal.Reddit.Core.Constants;
using RestSharp;
using Serilog;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Neal.Reddit.Client.Helpers;

/// <summary>
/// Represents a request rate limiter to maximize the number of requests possible during a reset period.
/// </summary>
public class RateLimiter : IDisposable
{
    private readonly SemaphoreSlim requestPool;

    private readonly TimeSpan resetTimeSpan;

    private readonly ConcurrentQueue<TimeSpan> releaseTimes;

    private readonly double minimumDelay;

    private readonly Stopwatch stopwatch = Stopwatch.StartNew();

    public RateLimiter(
        int maxRequests, 
        int resetPeriod)
    {
        this.requestPool = new SemaphoreSlim(maxRequests, maxRequests);
        this.resetTimeSpan = TimeSpan.FromSeconds(resetPeriod);
        this.minimumDelay = (maxRequests / resetPeriod) + Defaults.ReteLimitDelay;
        this.releaseTimes = new ConcurrentQueue<TimeSpan>();
        
        for (var i = 0; i < maxRequests; i++)
        {
            this.releaseTimes.Enqueue(TimeSpan.FromSeconds(i * minimumDelay));
        }
    }

    public void Dispose()
    {
        this.requestPool.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Run the <paramref name="action"/> when an opening is available.
    /// </summary>
    /// <typeparam name="T">Return <see cref="Type"/> of the <see cref="Task"/></typeparam>
    /// <param name="action"><see cref="Task"/> to execute</param>
    /// <param name="cancellationToken"></param>
    /// <returns><paramref name="action"/> as <see cref="Task"/> of <typeparamref name="T"/></returns>
    public async Task<T> Run<T>(Task<T> action, CancellationToken cancellationToken)
    {
        await this.Wait(cancellationToken);

        try
        {
            return await action;
        }
        finally
        {
            this.Release();
        }
    }

    private async Task Wait(CancellationToken cancellationToken)
    {        
        await this.requestPool.WaitAsync(cancellationToken);

        _ = this.releaseTimes.TryDequeue(out var oldestRelease);

        var elapsed = stopwatch.Elapsed - oldestRelease;
        var delay = TimeSpan.FromSeconds(this.minimumDelay) - elapsed;

        // Wait minimum delay
        if (delay > TimeSpan.Zero)
        {
            Log.Information("Delaying {delay} milliseconds for semaphore.", delay.TotalMilliseconds);
            await Task.Delay(delay, cancellationToken);
        }
    }

    private void Release()
    {
        this.releaseTimes.Enqueue(this.stopwatch.Elapsed);
        this.requestPool.Release();
    }
}
