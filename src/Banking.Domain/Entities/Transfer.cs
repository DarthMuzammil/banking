using Banking.Domain.Enums;

namespace Banking.Domain.Entities;

public class Transfer
{
    public Guid Id { get; set; }
    public Guid FromAccountId { get; set; }
    public Guid ToAccountId { get; set; }
    public decimal Amount { get; set; }
    public TransferStatus Status { get; set; }
    public string Reference { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
