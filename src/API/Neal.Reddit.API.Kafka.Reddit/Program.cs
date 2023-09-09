﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Neal.Reddit.API.Kafka.Reddit.Endpoints;
using Neal.Reddit.Application.Constants;
using Neal.Reddit.Application.Constants.Messages;
using Neal.Reddit.Application.Interfaces.RedditRepository;
using Serilog;
using static System.Net.Mime.MediaTypeNames;

try
{
    var builder = WebApplication
        .CreateBuilder(args);

    builder
        .Logging
        .ClearProviders()
        .AddSerilog();

    // Attach Serilog logger
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .CreateLogger();

    Log.Information(ApplicationStatusMessages.Started);

    if (!builder.Environment.IsProduction())
    {
        builder
            .Services
            .AddCors(options =>
            {
                options.AddPolicy("LocalhostPolicy",
                    builder => builder
                        .AllowAnyMethod()
                        .AllowCredentials()
                        .SetIsOriginAllowed((host) =>
                            host.Contains("localhost")
                            || host.Contains("::1")
                            || host.Contains("127.0.0.1")
                            || host.Contains("0.0.0.0")) // Allow localhost addresses
                        .AllowAnyHeader());
            });
    }

    builder
        .Services
        .AddEndpointsApiExplorer()
        .AddSwaggerGen()
        .AddMemoryCache()
        .AddHealthChecks();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsProduction())
    {
        app
            .UseCors("LocalhostPolicy")
            .UseDeveloperExceptionPage()
            .UseSwagger()
            .UseSwaggerUI()
            .UseStatusCodePages(Text.Plain, "Server returned status code: {0}");
    }
    else
    {
        app
            .UseExceptionHandler("/Error")
            .UseHsts();
    }

    app
        .UseHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => false
        })
        .UseRouting()
        .UseEndpoints(configuration =>
            configuration.MapRepositoryEndpoints(Names.KafkaApi));

    await app.RunAsync();
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