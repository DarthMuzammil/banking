using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Banking.Api.Filters;
using Banking.Application.Accounts.Commands;
using Banking.Application.Accounts.Queries;
using Banking.Application.Transactions.Commands;
using Banking.Application.Transactions.Queries;
using Banking.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Banking.Api.Controllers;

[ApiController]
[Route("api/accounts")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly CreateAccountCommandHandler _createAccountHandler;
    private readonly GetCustomerAccountsQueryHandler _getCustomerAccountsHandler;
    private readonly GetAccountByIdQueryHandler _getAccountByIdHandler;
    private readonly DepositCommandHandler _depositHandler;
    private readonly WithdrawCommandHandler _withdrawHandler;
    private readonly GetAccountTransactionsQueryHandler _getTransactionsHandler;

    public AccountsController(
        CreateAccountCommandHandler createAccountHandler,
        GetCustomerAccountsQueryHandler getCustomerAccountsHandler,
        GetAccountByIdQueryHandler getAccountByIdHandler,
        DepositCommandHandler depositHandler,
        WithdrawCommandHandler withdrawHandler,
        GetAccountTransactionsQueryHandler getTransactionsHandler)
    {
        _createAccountHandler = createAccountHandler;
        _getCustomerAccountsHandler = getCustomerAccountsHandler;
        _getAccountByIdHandler = getAccountByIdHandler;
        _depositHandler = depositHandler;
        _withdrawHandler = withdrawHandler;
        _getTransactionsHandler = getTransactionsHandler;
    }

    [HttpGet]
    public async Task<IActionResult> GetAccounts(CancellationToken cancellationToken)
    {
        if (!TryGetCustomerId(out var customerId))
        {
            return Unauthorized();
        }

        var result = await _getCustomerAccountsHandler.HandleAsync(
            new GetCustomerAccountsQuery(customerId),
            cancellationToken);

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAccount(
        [FromBody] CreateAccountRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCustomerId(out var customerId))
        {
            return Unauthorized();
        }

        if (!Enum.TryParse<AccountType>(request.AccountType, ignoreCase: true, out var accountType)
            || accountType is not (AccountType.Checking or AccountType.Savings))
        {
            return BadRequest(new
            {
                error = "Account type must be Checking or Savings.",
                code = "INVALID_ACCOUNT_TYPE"
            });
        }

        var command = new CreateAccountCommand(customerId, accountType);
        var result = await _createAccountHandler.HandleAsync(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error, code = result.ErrorCode });
        }

        return CreatedAtAction(nameof(GetAccount), new { id = result.Value!.AccountId }, result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAccount(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetCustomerId(out var customerId))
        {
            return Unauthorized();
        }

        var result = await _getAccountByIdHandler.HandleAsync(
            new GetAccountByIdQuery(customerId, id),
            cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.Error, code = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    [HttpPost("{id:guid}/deposits")]
    [Idempotent]
    public async Task<IActionResult> Deposit(
        Guid id,
        [FromBody] DepositRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCustomerId(out var customerId))
        {
            return Unauthorized();
        }

        var command = new DepositCommand(customerId, id, request.Amount, request.Description);
        var result = await _depositHandler.HandleAsync(command, cancellationToken);

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

    [HttpPost("{id:guid}/withdrawals")]
    [Idempotent]
    public async Task<IActionResult> Withdraw(
        Guid id,
        [FromBody] WithdrawRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCustomerId(out var customerId))
        {
            return Unauthorized();
        }

        var command = new WithdrawCommand(customerId, id, request.Amount, request.Description);
        var result = await _withdrawHandler.HandleAsync(command, cancellationToken);

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

    [HttpGet("{id:guid}/transactions")]
    public async Task<IActionResult> GetTransactions(
        Guid id,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetCustomerId(out var customerId))
        {
            return Unauthorized();
        }

        var query = new GetAccountTransactionsQuery(customerId, id, skip, take);
        var result = await _getTransactionsHandler.HandleAsync(query, cancellationToken);

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

public sealed record CreateAccountRequest(string AccountType);
public sealed record DepositRequest(decimal Amount, string? Description);
public sealed record WithdrawRequest(decimal Amount, string? Description);