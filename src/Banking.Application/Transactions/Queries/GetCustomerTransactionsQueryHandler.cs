using Banking.Application.Abstractions;
using Banking.Application.Common;
using Banking.Application.Transactions.DTOs;

namespace Banking.Application.Transactions.Queries;

public sealed class GetCustomerTransactionsQueryHandler
{
    private const int DefaultTake = 50;
    private const int MaxTake = 100;

    private readonly ITransactionRepository _transactionRepository;

    public GetCustomerTransactionsQueryHandler(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<Result<CustomerTransactionsPageDto>> HandleAsync(
        GetCustomerTransactionsQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.Skip < 0)
        {
            return Result<CustomerTransactionsPageDto>.Failure(
                "Skip must be zero or greater.",
                "INVALID_SKIP");
        }

        var take = query.Take <= 0 ? DefaultTake : Math.Min(query.Take, MaxTake);

        var rows = await _transactionRepository.GetByCustomerIdAsync(
            query.CustomerId,
            query.AccountId,
            query.Search,
            query.Skip,
            take,
            cancellationToken);

        var items = rows
            .Select(r => new CustomerTransactionDto(
                r.Transaction.Id,
                r.Transaction.AccountId,
                r.AccountNumber,
                r.Transaction.Type,
                r.Transaction.Amount,
                r.Transaction.BalanceAfter,
                r.Transaction.Description,
                r.Transaction.CreatedAt))
            .ToList();

        return Result<CustomerTransactionsPageDto>.Success(
            new CustomerTransactionsPageDto(query.Skip, take, items));
    }
}
