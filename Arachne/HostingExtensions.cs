using Serilog;

namespace Arachne;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
            
        builder.Services.AddRazorPages();
        builder.Services.AddCors(setup =>
        {
            setup.AddDefaultPolicy(policy => policy.AllowAnyOrigin());
        });

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        var isBuilder = builder.Services
            .AddIdentityServer(options =>
            {
                options.EmitStaticAudienceClaim = true;
            })
            .AddInMemoryIdentityResources(Config.IdentityResources)
            .AddInMemoryApiScopes(Config.ApiScopes)
            .AddInMemoryClients(Config.Clients)
            .AddTestUsers(TestUsers.Users)
            ;

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();
        
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();

            app.UseCors(config =>
            {
                config.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            });
        }

        app.UseStaticFiles();
        app.UseRouting();
            
        app.UseIdentityServer();

        app.UseAuthorization();
        app.MapRazorPages().RequireAuthorization();

        return app;
    }
}