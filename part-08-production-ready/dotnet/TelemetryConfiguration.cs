using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Microsoft.Extensions.DependencyInjection;

namespace MAF.Part08.Telemetry;

/// <summary>
/// Part 8: OpenTelemetry Configuration for .NET
/// </summary>
public static class TelemetryConfiguration
{
    public static IServiceCollection AddAgentTelemetry(
        this IServiceCollection services,
        string serviceName = "agent-service")
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: serviceName,
                    serviceVersion: "1.0.0")
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = 
                        Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "development"
                }))
            .WithTracing(tracing => tracing
                .AddSource("Microsoft.Agents.AI")
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(
                        Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT") 
                        ?? "http://localhost:4317");
                }))
            .WithMetrics(metrics => metrics
                .AddMeter("Microsoft.Agents.AI")
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(
                        Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT") 
                        ?? "http://localhost:4317");
                }));
        
        return services;
    }
}

// Program.cs usage example
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add telemetry
        builder.Services.AddAgentTelemetry("customer-support-agent");

        var app = builder.Build();

        app.MapPost("/api/agent/chat", async (ChatRequest request) =>
        {
            // Agent endpoints are automatically traced
            return Results.Ok(new { response = "Hello!" });
        });

        app.Run();
    }
}

public record ChatRequest(string Message);
