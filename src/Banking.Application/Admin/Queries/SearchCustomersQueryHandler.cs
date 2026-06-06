using Banking.Application.Abstractions;
using Banking.Application.Admin.DTOs;
using Banking.Domain.Enums;

namespace Banking.Application.Admin.Queries;

public sealed class SearchCustomersQueryHandler
{
    private readonly ICustomerRepository _customerRepository;

    public SearchCustomersQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<IReadOnlyList<AdminCustomerDto>> HandleAsync(
        SearchCustomersQuery query,
        CancellationToken cancellationToken = default)
    {
        var customers = await _customerRepository.SearchAsync(
            query.Search ?? string.Empty,
            query.Take,
            cancellationToken);

        return customers
            .Select(c => new AdminCustomerDto(
                c.Id,
                c.Email,
                c.FirstName,
                c.LastName,
                c.Role.ToString(),
                c.CreatedAt))
            .ToList();
    }
}
