using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Neal.Reddit.Application.Constants.Messages;
using Neal.Reddit.Infrastructure.Reader.Services.RedditApi.V1;

// Define thread to periodically send log messages for heartbeats
var heartbeatThread = new Thread(new ThreadStart(() =>
{
    while (true)
    {
        Thread.Sleep(TimeSpan.FromMinutes(1));
        Log.Information($"{nameof(RedditReaderService)} heartbeat good");
    }
}));

try
{
    // Load configuration
    var configuration = new ConfigurationBuilder()
        .AddJsonFile($"appsettings.json", false, true)
        .AddEnvironmentVariables()
        .AddUserSecrets<Program>()
        .Build();

    // Attach Serilog logger
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(configuration)
        .CreateLogger();

    Log.Information(ApplicationStatusMessages.Started);

    // Configure and build Host Service
    var host = Host.CreateDefaultBuilder(args)
        .ConfigureServices((context, services) =>
        {
            services
                .Configure<HostOptions>(options => options.ShutdownTimeout = TimeSpan.FromSeconds(10))
                .Configure<JsonSerializerOptions>(options =>
                {
                    options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.DefaultIgnoreCondition = JsonIgnoreCondition.Always;
                })
                .AddLogging(options =>
                {
                    options.ClearProviders();
                    options.AddSerilog(dispose: true);
                })
                .AddKafkaClient();

            services
                .AddHostedService<RedditReaderService>();
        })
        .UseSerilog()
        .Build();

    Log.Information(ApplicationStatusMessages.HostedServiceStarting);

    heartbeatThread.Start();

    // Start hosted service execution
    await host.RunAsync();

    Log.Information(ApplicationStatusMessages.HostedServiceFinished);
}
catch (Exception ex)
{
    // Write to console in case log initialization fails
    Console.WriteLine($"{DateTime.UtcNow}Unexpected exception occurred:\n{ex}");
    Log.Fatal(ex, ApplicationStatusMessages.FatalError);
}
// Clean up application and ensure log is flushed
finally
{
    Log.Information(ApplicationStatusMessages.Stopped);
    Log.CloseAndFlush();
}