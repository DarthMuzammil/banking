using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Banking.Api.Filters;
using Banking.Application.Transfers.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Banking.Api.Controllers;

[ApiController]
[Route("api/transfers")]
[Authorize]
public class TransfersController : ControllerBase
{
    private readonly TransferCommandHandler _transferHandler;

    public TransfersController(TransferCommandHandler transferHandler)
    {
        _transferHandler = transferHandler;
    }

    [HttpPost]
    [Idempotent]
    public async Task<IActionResult> Transfer(
        [FromBody] TransferRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCustomerId(out var customerId))
        {
            return Unauthorized();
        }

        var command = new TransferCommand(
            customerId,
            request.FromAccountId,
            request.ToAccountId,
            request.Amount,
            request.Description);

        var result = await _transferHandler.HandleAsync(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "ACCOUNT_NOT_FOUND" => NotFound(new { error = result.Error, code = result.ErrorCode }),
                _ => BadRequest(new { error = result.Error, code = result.ErrorCode })
            };
        }

        return Ok(result.Value);
    }

    private bool TryGetCustomerId(out Guid customerId)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return Guid.TryParse(sub, out customerId);
    }
}

public sealed record TransferRequest(
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    string? Description);
