using System.Net;
using System.Text.Json;
using Banking.Domain.Exceptions;

namespace Banking.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict for {ResourceType} {ResourceId}",
                ex.ResourceType,
                ex.ResourceId);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.Conflict;

            var payload = JsonSerializer.Serialize(new
            {
                error = "The resource was modified by another request. Please retry.",
                code = "CONCURRENCY_CONFLICT"
            });

            await context.Response.WriteAsync(payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var payload = JsonSerializer.Serialize(new
            {
                error = "An unexpected error occurred.",
                code = "INTERNAL_ERROR"
            });

            await context.Response.WriteAsync(payload);
        }
    }
}
