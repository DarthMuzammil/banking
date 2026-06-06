using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Banking.Application.Settings.Commands;
using Banking.Application.Settings.DTOs;
using Banking.Application.Settings.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Banking.Api.Controllers;

[ApiController]
[Route("api/settings")]
[Authorize]
public class SettingsController : ControllerBase
{
    private readonly GetSettingsQueryHandler _getHandler;
    private readonly UpdateSettingsCommandHandler _updateHandler;

    public SettingsController(
        GetSettingsQueryHandler getHandler,
        UpdateSettingsCommandHandler updateHandler)
    {
        _getHandler = getHandler;
        _updateHandler = updateHandler;
    }

    [HttpGet]
    public async Task<IActionResult> GetSettings(CancellationToken cancellationToken)
    {
        if (!TryGetCustomerId(out var customerId))
        {
            return Unauthorized();
        }

        var result = await _getHandler.HandleAsync(new GetSettingsQuery(customerId), cancellationToken);
        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.Error, code = result.ErrorCode });
        }

        return Ok(MapResponse(result.Value!));
    }

    [HttpPatch]
    public async Task<IActionResult> UpdateSettings(
        [FromBody] UpdateSettingsRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCustomerId(out var customerId))
        {
            return Unauthorized();
        }

        var dto = new UpdateSettingsDto(
            request.Email,
            request.FirstName,
            request.LastName,
            request.TwoFactorEnabled,
            new NotificationSettingsDto(
                request.Notifications.Transactions,
                request.Notifications.Security,
                request.Notifications.Marketing));

        var result = await _updateHandler.HandleAsync(
            new UpdateSettingsCommand(customerId, dto),
            cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error, code = result.ErrorCode });
        }

        return Ok(MapResponse(result.Value!));
    }

    private static object MapResponse(SettingsDto dto) => new
    {
        email = dto.Email,
        firstName = dto.FirstName,
        lastName = dto.LastName,
        twoFactorEnabled = dto.TwoFactorEnabled,
        notifications = new
        {
            transactions = dto.Notifications.Transactions,
            security = dto.Notifications.Security,
            marketing = dto.Notifications.Marketing
        }
    };

    private bool TryGetCustomerId(out Guid customerId)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return Guid.TryParse(sub, out customerId);
    }
}

public sealed record UpdateSettingsRequest(
    string Email,
    string FirstName,
    string LastName,
    bool TwoFactorEnabled,
    NotificationRequest Notifications);

public sealed record NotificationRequest(bool Transactions, bool Security, bool Marketing);
