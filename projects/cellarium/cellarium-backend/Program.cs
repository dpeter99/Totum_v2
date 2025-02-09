using cellarium_backend;
using cellarium_backend.Models;
using cellarium_backend.Services;
using cellarium_backend.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

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
        options.Events.OnTokenValidated = context =>
        {
            
            return Task.CompletedTask;
        };
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

var app = builder.Build();

app.UseCors(config =>
{
    config.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();

app.UseHttpsRedirection();

app.MapControllers();



app.Run();
