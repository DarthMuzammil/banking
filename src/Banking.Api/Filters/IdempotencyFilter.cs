using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Banking.Application.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Banking.Api.Filters;

[AttributeUsage(AttributeTargets.Method)]
public sealed class IdempotentAttribute : Attribute, IFilterFactory
{
    public bool IsReusable => true;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider) =>
        serviceProvider.GetRequiredService<IdempotencyFilter>();
}

public sealed class IdempotencyFilter : IAsyncActionFilter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IIdempotencyRepository _idempotencyRepository;

    public IdempotencyFilter(IIdempotencyRepository idempotencyRepository)
    {
        _idempotencyRepository = idempotencyRepository;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var idempotencyKey = context.HttpContext.Request.Headers["Idempotency-Key"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            await next();
            return;
        }

        if (!TryGetCustomerId(context.HttpContext.User, out var customerId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var requestPath = context.HttpContext.Request.Path.Value ?? string.Empty;
        var cached = await _idempotencyRepository.GetAsync(
            customerId,
            idempotencyKey.Trim(),
            requestPath,
            context.HttpContext.RequestAborted);

        if (cached is not null)
        {
            context.Result = new ContentResult
            {
                Content = cached.Body,
                ContentType = "application/json",
                StatusCode = cached.StatusCode
            };
            return;
        }

        var executed = await next();

        if (executed.Result is not ObjectResult objectResult || objectResult.StatusCode is null or < 200 or >= 300)
        {
            return;
        }

        var body = objectResult.Value is null
            ? "null"
            : JsonSerializer.Serialize(objectResult.Value, JsonOptions);

        await _idempotencyRepository.StoreAsync(
            customerId,
            idempotencyKey.Trim(),
            requestPath,
            objectResult.StatusCode.Value,
            body,
            context.HttpContext.RequestAborted);
    }

    private static bool TryGetCustomerId(ClaimsPrincipal user, out Guid customerId)
    {
        var sub = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return Guid.TryParse(sub, out customerId);
    }
}
