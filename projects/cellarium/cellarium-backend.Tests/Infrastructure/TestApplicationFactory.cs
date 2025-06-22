using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Cellarium.Tests.Infrastructure;

/// <summary>
/// Test application factory that provides a configured test environment for API testing.
/// Includes in-memory database, telemetry collection, and test authentication.
/// </summary>
public class TestApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    private readonly string _testDbName = $"TestDb_{DateTime.Now.Ticks}";
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace production database with in-memory test database
            RemoveService<DbContextOptions<cellarium_backend.CellariumDbContext>>(services);
            
            services.AddDbContext<cellarium_backend.CellariumDbContext>(options =>
            {
                options.UseInMemoryDatabase(_testDbName);
            });

            // Configure OpenTelemetry for testing with span collection
            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService("Cellarium-API-Tests"))
                .WithTracing(tracing =>
                {
                    tracing
                        .AddSource("cellarium-backend")
                        .AddAspNetCoreInstrumentation()
                        .AddInMemoryExporter(TelemetryCollector.CollectedSpans);
                });

            // Ensure test database is ready
            EnsureTestDatabaseCreated(services);
        });

        builder.UseEnvironment("Development");
    }

    private static void RemoveService<T>(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(T));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }
    }

    private static void EnsureTestDatabaseCreated(IServiceCollection services)
    {
        using var scope = services.BuildServiceProvider().CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<cellarium_backend.CellariumDbContext>();
        context.Database.EnsureCreated();
    }
}