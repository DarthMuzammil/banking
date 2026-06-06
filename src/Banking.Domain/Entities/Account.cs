using Banking.Domain.Enums;

namespace Banking.Domain.Entities;

public class Account
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public AccountType AccountType { get; set; }
    public decimal Balance { get; set; }
    public string Currency { get; set; } = "USD";
    public AccountStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
