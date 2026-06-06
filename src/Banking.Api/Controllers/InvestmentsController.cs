using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Banking.Application.Investments.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Banking.Api.Controllers;

[ApiController]
[Route("api/investments")]
[Authorize]
public class InvestmentsController : ControllerBase
{
    private readonly GetPortfolioQueryHandler _handler;

    public InvestmentsController(GetPortfolioQueryHandler handler)
    {
        _handler = handler;
    }

    [HttpGet("portfolio")]
    public async Task<IActionResult> GetPortfolio(CancellationToken cancellationToken)
    {
        if (!TryGetCustomerId(out var customerId))
        {
            return Unauthorized();
        }

        var portfolio = await _handler.HandleAsync(new GetPortfolioQuery(customerId), cancellationToken);

        return Ok(new
        {
            totalValue = portfolio.TotalValue,
            dayChange = portfolio.DayChange,
            dayChangePercent = portfolio.DayChangePercent,
            holdings = portfolio.Holdings.Select(h => new
            {
                id = h.Id,
                symbol = h.Symbol,
                name = h.Name,
                shares = h.Shares,
                value = h.Value,
                changePercent = h.ChangePercent
            })
        });
    }

    private bool TryGetCustomerId(out Guid customerId)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return Guid.TryParse(sub, out customerId);
    }
}
