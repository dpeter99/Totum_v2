using Microsoft.Extensions.Hosting;
using RootHost.Utils;

namespace RootHost;

public static class PnpmProjectExtensions
{
    public static IResourceBuilder<NodeAppResource> AddPnpmApp(this IDistributedApplicationBuilder builder, [ResourceName] string name, string workingDirectory, string scriptName = "start", string[]? args = null)
    {

        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(workingDirectory);
        ArgumentNullException.ThrowIfNull(scriptName);

        string[] allArgs = args is { Length: > 0 }
            ? ["run", scriptName, "--", .. args]
            : ["run", scriptName];

        workingDirectory = PathNormalizer.NormalizePathForCurrentPlatform(Path.Combine(builder.AppHostDirectory, workingDirectory));
        var resource = new NodeAppResource(name, "pnpm", workingDirectory);

        return builder.AddResource(resource)
            .WithNodeDefaults()
            .WithArgs(allArgs);
    }
    
    private static IResourceBuilder<NodeAppResource> WithNodeDefaults(this IResourceBuilder<NodeAppResource> builder) =>
        builder.WithOtlpExporter()
            .WithEnvironment("NODE_ENV", builder.ApplicationBuilder.Environment.IsDevelopment() ? "development" : "production");
    
}