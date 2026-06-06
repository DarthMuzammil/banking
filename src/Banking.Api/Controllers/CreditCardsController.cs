using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Banking.Application.CreditCards.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Banking.Api.Controllers;

[ApiController]
[Route("api/credit-cards")]
[Authorize]
public class CreditCardsController : ControllerBase
{
    private readonly GetCreditCardsQueryHandler _cardsHandler;
    private readonly GetCardSpendingQueryHandler _spendingHandler;

    public CreditCardsController(
        GetCreditCardsQueryHandler cardsHandler,
        GetCardSpendingQueryHandler spendingHandler)
    {
        _cardsHandler = cardsHandler;
        _spendingHandler = spendingHandler;
    }

    [HttpGet]
    public async Task<IActionResult> GetCreditCards(CancellationToken cancellationToken)
    {
        if (!TryGetCustomerId(out var customerId))
        {
            return Unauthorized();
        }

        var cards = await _cardsHandler.HandleAsync(new GetCreditCardsQuery(customerId), cancellationToken);

        return Ok(cards.Select(c => new
        {
            id = c.Id,
            name = c.Name,
            lastFour = c.LastFour,
            balance = c.Balance,
            limit = c.Limit,
            dueDate = c.DueDate.ToString("yyyy-MM-dd"),
            minPayment = c.MinPayment,
            apr = c.Apr
        }));
    }

    [HttpGet("{id:guid}/spending")]
    public async Task<IActionResult> GetCardSpending(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetCustomerId(out var customerId))
        {
            return Unauthorized();
        }

        var result = await _spendingHandler.HandleAsync(
            new GetCardSpendingQuery(customerId, id),
            cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.Error, code = result.ErrorCode });
        }

        return Ok(result.Value!.Select(s => new
        {
            category = s.Category,
            amount = s.Amount,
            percentage = s.Percentage
        }));
    }

    private bool TryGetCustomerId(out Guid customerId)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return Guid.TryParse(sub, out customerId);
    }
}
