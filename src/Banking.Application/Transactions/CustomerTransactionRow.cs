using Banking.Domain.Entities;

namespace Banking.Application.Transactions;

public sealed record CustomerTransactionRow(Transaction Transaction, string AccountNumber);
