using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Banking.Application.Payments.Commands;
using Banking.Application.Payments.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Banking.Api.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly GetBillPaymentsQueryHandler _billsHandler;
    private readonly GetScheduledPaymentsQueryHandler _scheduledHandler;
    private readonly PayBillCommandHandler _payBillHandler;

    public PaymentsController(
        GetBillPaymentsQueryHandler billsHandler,
        GetScheduledPaymentsQueryHandler scheduledHandler,
        PayBillCommandHandler payBillHandler)
    {
        _billsHandler = billsHandler;
        _scheduledHandler = scheduledHandler;
        _payBillHandler = payBillHandler;
    }

    [HttpGet("bills")]
    public async Task<IActionResult> GetBills(CancellationToken cancellationToken)
    {
        if (!TryGetCustomerId(out var customerId))
        {
            return Unauthorized();
        }

        var bills = await _billsHandler.HandleAsync(new GetBillPaymentsQuery(customerId), cancellationToken);

        return Ok(bills.Select(b => new
        {
            id = b.Id,
            payee = b.Payee,
            amount = b.Amount,
            dueDate = b.DueDate.ToString("yyyy-MM-dd"),
            status = b.Status,
            accountId = b.AccountId
        }));
    }

    [HttpPost("bills/{id:guid}/pay")]
    public async Task<IActionResult> PayBill(
        Guid id,
        [FromBody] PayBillRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCustomerId(out var customerId))
        {
            return Unauthorized();
        }

        var result = await _payBillHandler.HandleAsync(
            new PayBillCommand(customerId, id, request.AccountId),
            cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "BILL_NOT_FOUND" or "ACCOUNT_NOT_FOUND" => NotFound(new { error = result.Error, code = result.ErrorCode }),
                _ => BadRequest(new { error = result.Error, code = result.ErrorCode })
            };
        }

        return Ok(result.Value);
    }

    [HttpGet("scheduled")]
    public async Task<IActionResult> GetScheduled(CancellationToken cancellationToken)
    {
        if (!TryGetCustomerId(out var customerId))
        {
            return Unauthorized();
        }

        var items = await _scheduledHandler.HandleAsync(
            new GetScheduledPaymentsQuery(customerId),
            cancellationToken);

        return Ok(items.Select(s => new
        {
            id = s.Id,
            payee = s.Payee,
            amount = s.Amount,
            frequency = s.Frequency,
            nextDate = s.NextDate.ToString("yyyy-MM-dd"),
            accountId = s.AccountId
        }));
    }

    private bool TryGetCustomerId(out Guid customerId)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return Guid.TryParse(sub, out customerId);
    }
}

public sealed record PayBillRequest(Guid AccountId);
