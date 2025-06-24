using cellarium_backend;
using cellarium_backend.Features.ShoppingLists.Services;
using cellarium_backend.Shared.Services.Auth;
using cellarium_backend.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers();

builder.Services.AddCors();

builder.Services.AddDbContext<CellariumDbContext>(options =>
    options.UseInMemoryDatabase("cellarium"));

builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        options.Authority = "https://localhost:5001";
        options.TokenValidationParameters.ValidateAudience = false;
        //options.Events.OnTokenValidated = context => Task.CompletedTask;
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .RequireClaim("scope", "cellarium")
        .Build();
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddTransient<IShoppingListService, ShoppingListService>();
builder.Services.AddTransient<IShoppingListItemService, ShoppingListItemService>();

var app = builder.Build();

app.UseCors(config =>
{
    config.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
});

// Add business logic exception handling middleware
app.UseMiddleware<BusinessLogicExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();

app.UseHttpsRedirection();

app.MapControllers();



app.Run();

public static class ActivityHelper
{
    public static ActivitySource Source = new ActivitySource("cellarium-backend");
}

public partial class Program { }
