using System.Reflection;
using MartinCostello.OpenApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ServiceDiscovery;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;

namespace Microsoft.Extensions.Hosting;

// Adds common .NET Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
public static class Extensions
{
    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();

            // Turn on service discovery by default
            http.AddServiceDiscovery();
        });

        builder.AddOpenApiDefaults();
        
        return builder;
    }

    public static IHostApplicationBuilder AddOpenApiDefaults(this IHostApplicationBuilder builder)
    {
        builder.Services.AddOpenApi(options =>
        {
            options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;
            options.AddDocumentTransformer((document, context, token) =>
            {
                document.Info.Contact = new OpenApiContact()
                {
                    Name = "dpeter99",
                };
                document.Info.Description = "API";
                document.Servers = new List<OpenApiServer>()
                {
                    new OpenApiServer()
                    {
                        Url = context.ApplicationServices.GetService<IHostEnvironment>()?.ContentRootPath,
                    }
                };
                
                // Configure bearer authentication using a JWT
                var scheme = new OpenApiSecurityScheme()
                {
                    BearerFormat = "JSON Web Token",
                    Description = "Bearer authentication using a JWT.",
                    Type = SecuritySchemeType.OAuth2,
                    Scheme = "code",
                    Flows = new OpenApiOAuthFlows()
                    {
                         AuthorizationCode = new OpenApiOAuthFlow()
                        {
                            AuthorizationUrl = new Uri("https://localhost:5001/connect/authorize"),
                            TokenUrl = new Uri("https://localhost:5001/connect/token"),
                            Scopes = new Dictionary<string, string>()
                            {
                                { "cellarium", "cellarium" },
                            },
                        }
                    },
                    Reference = new()
                    {
                        Id = "UserAuth",
                        Type = ReferenceType.SecurityScheme,
                    },
                };

                document.Components ??= new();
                document.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();
                document.Components.SecuritySchemes[scheme.Reference.Id] = scheme;
                document.SecurityRequirements ??= [];
                document.SecurityRequirements.Add(new() { [scheme] = [] });
                
                return Task.CompletedTask;
            });

            options.AddSchemaTransformer((document, context, token) =>
            {
                var customAttributeProvider = context.JsonPropertyInfo?.AttributeProvider;
                if(customAttributeProvider != null &&
                   customAttributeProvider.IsDefined(typeof(BrandedTypeAttribute), false))
                {
                    var attribute = customAttributeProvider.GetCustomAttributes(typeof(BrandedTypeAttribute), false)
                        .OfType<BrandedTypeAttribute>().First();
                    document.Format = $"brand::{attribute.Brand}";
                }
                return Task.CompletedTask;
            });
        });
        
        builder.Services.AddOpenApiExtensions(options =>
        {
            options.AddServerUrls = true;
        });
        
        builder.Services.AddHttpContextAccessor();
        
        return builder;
    }
    
    public static IHostApplicationBuilder ConfigureOpenTelemetry(this IHostApplicationBuilder builder)
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation()
                    // Uncomment the following line to enable gRPC instrumentation (requires the OpenTelemetry.Instrumentation.GrpcNetClient package)
                    //.AddGrpcClientInstrumentation()
                    .AddHttpClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static IHostApplicationBuilder AddOpenTelemetryExporters(this IHostApplicationBuilder builder)
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        return builder;
    }

    public static IHostApplicationBuilder AddDefaultHealthChecks(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks()
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Adding health checks endpoints to applications in non-development environments has security implications.
        // See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
        if (app.Environment.IsDevelopment())
        {
            // All health checks must pass for app to be considered ready to accept traffic after starting
            app.MapHealthChecks("/health")
                .AllowAnonymous();

            // Only health checks tagged with the "live" tag must pass for app to be considered alive
            app.MapHealthChecks("/alive", new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live")
            }).AllowAnonymous();

            app.AddOpenApi();
        }

        return app;
    }

    public static WebApplication AddOpenApi(this WebApplication app)
    {
        app.MapOpenApi().AllowAnonymous();
        app.MapScalarApiReference(options =>
        {
            options.Authentication ??= new();
            options.Authentication.OAuth2 = new()
            {
                ClientId = "cellarium-client",
                Scopes = new []
                {
                    "cellarium",
                }
            };
        }).AllowAnonymous();
        
        return app;
    }
}
