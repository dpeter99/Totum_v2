using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using cellarium_backend.Models;

namespace cellarium_backend.Tests;

public class CustomApplicationFactoryWithTelemetry<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    private readonly string _testDbName = $"TestDb_{DateTime.Now.Ticks}";
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<CellariumDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add a database context using an in-memory database for testing
            // Use a shared database name for all requests within the same test factory instance
            services.AddDbContext<CellariumDbContext>(options =>
            {
                options.UseInMemoryDatabase(_testDbName);
            });

            // Configure OpenTelemetry for testing
            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService("Cellarium-Test-App"))
                .WithTracing(tracing =>
                {
                    tracing
                        .AddSource("cellarium-backend")
                        .AddAspNetCoreInstrumentation()
                        .AddInMemoryExporter(OtelTestFramework.CollectedSpans);
                });

            // Ensure database is created
            using var scope = services.BuildServiceProvider().CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CellariumDbContext>();
            context.Database.EnsureCreated();
        });

        builder.UseEnvironment("Testing");
    }
}