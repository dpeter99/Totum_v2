using cellarium_backend;
using cellarium_backend.Models;
using cellarium_backend.Services;
using cellarium_backend.Services.Auth;
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
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "api1");
    });
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddTransient<IShoppingListService, ShoppingListService>();

var app = builder.Build();

app.UseCors(config =>
{
    config.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
});

app.MapDefaultEndpoints();

app.UseHttpsRedirection();

app.MapControllers();


app.Run();
