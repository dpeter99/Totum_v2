using PracticalOtel.xUnit.OpenTelemetry;
using Xunit.Abstractions;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Cellarium.Tests.Infrastructure;

/// <summary>
/// Test framework that sets up OpenTelemetry tracing for API tests.
/// Integrates with the testing infrastructure to collect and validate telemetry.
/// </summary>
public class CellariumTestFramework : TracedTestFramework
{
    public CellariumTestFramework(IMessageSink messageSink) : base(messageSink)
    {
        traceProviderSetup = tpb => {
            tpb
                .ConfigureResource(resource => resource.AddService("Cellarium-API-Tests"))
                .AddSource("UnitTests")
                .AddSource("cellarium-backend")
                .AddInMemoryExporter(TelemetryCollector.CollectedSpans)
                .AddAspNetCoreInstrumentation()
                .AddOtlpExporter();
        };
    }
}