using Banking.Domain.Enums;

namespace Banking.Domain.Entities;

public class Transaction
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public string? Description { get; set; }
    public Guid? ReferenceId { get; set; }
    public TransactionCategory? Category { get; set; }
    public DateTime CreatedAt { get; set; }
}
