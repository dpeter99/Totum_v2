using cellarium_backend.Exceptions;
using System.Net;
using System.Text.Json;

namespace cellarium_backend.Middleware;

/// <summary>
/// Middleware that catches business logic exceptions and converts them to appropriate HTTP responses.
/// </summary>
public class BusinessLogicExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public BusinessLogicExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (BusinessLogicException ex)
        {
            await HandleBusinessLogicExceptionAsync(context, ex);
        }
    }

    private static async Task HandleBusinessLogicExceptionAsync(HttpContext context, BusinessLogicException ex)
    {
        context.Response.ContentType = "application/json";

        var response = ex switch
        {
            DuplicateListNameException duplicateEx => new
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = ex.Message,
                Details = $"A shopping list with the name '{duplicateEx.ListName}' already exists for this user."
            },
            _ => new
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = ex.Message,
                Details = "A business logic validation error occurred."
            }
        };

        context.Response.StatusCode = response.StatusCode;

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}