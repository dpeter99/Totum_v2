using Aspire.Hosting;
using Projects;
using RootHost;

var builder = DistributedApplication.CreateBuilder(args);

var arachne = builder.AddProject<Projects.Arachne>("Arachne");

var cellariumBackend =
    builder.AddProject<Projects.cellarium_backend>("cellarium-backend", "https")
        .WithReference(arachne);

builder.AddPnpmApp(
        name: "cellarium-frontend",
        workingDirectory: "../cellarium/cellarium-frontend",
        scriptName: "dev")
    .WithHttpsEndpoint(port: 6001, env: "PORT")
    .WithExternalHttpEndpoints()
    .WithReference(arachne)
    .WithReference(cellariumBackend);

builder.Build().Run();