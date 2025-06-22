using PracticalOtel.xUnit.OpenTelemetry;
using Xunit.Abstractions;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace cellarium_backend.Tests;

public class OtelTestFramework : TracedTestFramework
{
    public static readonly InMemoryTestSpans CollectedSpans = [];
    
    public OtelTestFramework(IMessageSink messageSink) : base(messageSink)
    {
        traceProviderSetup = tpb => {
            tpb
                .ConfigureResource(resource => resource.AddService("Cellarium-Tests"))
                .AddSource("UnitTests")
                .AddSource("cellarium-backend")  // Include app's ActivitySource
                .AddInMemoryExporter(CollectedSpans)    // Collect spans for testing
                .AddAspNetCoreInstrumentation()
                .AddOtlpExporter();
        };
    }
}