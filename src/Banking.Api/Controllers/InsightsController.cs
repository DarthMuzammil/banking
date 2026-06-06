using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Banking.Application.Insights.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Banking.Api.Controllers;

[ApiController]
[Route("api/insights")]
[Authorize]
public class InsightsController : ControllerBase
{
    private readonly GetSpendingInsightsQueryHandler _handler;

    public InsightsController(GetSpendingInsightsQueryHandler handler)
    {
        _handler = handler;
    }

    [HttpGet("spending")]
    public async Task<IActionResult> GetSpendingInsights(CancellationToken cancellationToken)
    {
        if (!TryGetCustomerId(out var customerId))
        {
            return Unauthorized();
        }

        var result = await _handler.HandleAsync(new GetSpendingInsightsQuery(customerId), cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error, code = result.ErrorCode });
        }

        return Ok(result.Value!.Select(i => new
        {
            id = i.Id,
            label = i.Label,
            amount = i.Amount,
            changePercent = i.ChangePercent,
            direction = i.Direction
        }));
    }

    private bool TryGetCustomerId(out Guid customerId)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return Guid.TryParse(sub, out customerId);
    }
}
