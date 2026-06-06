using Banking.Domain.Enums;

namespace Banking.Application.Insights;

public sealed record CategorySpendingRow(TransactionCategory Category, decimal Total);
