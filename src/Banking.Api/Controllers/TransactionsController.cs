using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Banking.Application.Transactions.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Banking.Api.Controllers;

[ApiController]
[Route("api/transactions")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly GetCustomerTransactionsQueryHandler _handler;

    public TransactionsController(GetCustomerTransactionsQueryHandler handler)
    {
        _handler = handler;
    }

    [HttpGet]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] Guid? accountId,
        [FromQuery] string? search,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetCustomerId(out var customerId))
        {
            return Unauthorized();
        }

        var result = await _handler.HandleAsync(
            new GetCustomerTransactionsQuery(customerId, accountId, search, skip, take),
            cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error, code = result.ErrorCode });
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
