using System.Text.Json;
using Aihrly.Api.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Aihrly.Api.Middleware;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteProblemResponseAsync(context, ex);
        }
    }

    private static async Task WriteProblemResponseAsync(HttpContext context, Exception ex)
    {
        var (status, title) = ex switch
        {
            NotFoundException => (StatusCodes.Status404NotFound,"Resource not found"),
            ConflictException => (StatusCodes.Status409Conflict,"Conflict"),
            BadRequestException => (StatusCodes.Status400BadRequest,"Bad request"),
            _  => (StatusCodes.Status500InternalServerError,"An unexpected error occurred")
        };

        var problem = new ProblemDetails
        {
            Title = title,
            Status = status,
            Detail = ex.Message
        };

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
