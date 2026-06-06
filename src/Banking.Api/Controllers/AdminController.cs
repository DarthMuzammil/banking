using Banking.Application.Admin.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Banking.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Staff")]
public class AdminController : ControllerBase
{
    private readonly SearchCustomersQueryHandler _searchCustomersHandler;
    private readonly GetCustomerAccountsAdminQueryHandler _getCustomerAccountsHandler;

    public AdminController(
        SearchCustomersQueryHandler searchCustomersHandler,
        GetCustomerAccountsAdminQueryHandler getCustomerAccountsHandler)
    {
        _searchCustomersHandler = searchCustomersHandler;
        _getCustomerAccountsHandler = getCustomerAccountsHandler;
    }

    [HttpGet("customers")]
    public async Task<IActionResult> SearchCustomers(
        [FromQuery] string? search,
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        var customers = await _searchCustomersHandler.HandleAsync(
            new SearchCustomersQuery(search, take),
            cancellationToken);

        return Ok(customers);
    }

    [HttpGet("customers/{customerId:guid}/accounts")]
    public async Task<IActionResult> GetCustomerAccounts(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        var result = await _getCustomerAccountsHandler.HandleAsync(
            new GetCustomerAccountsAdminQuery(customerId),
            cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.Error, code = result.ErrorCode });
        }

        return Ok(result.Value);
    }
}
