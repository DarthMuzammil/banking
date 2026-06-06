using Banking.Domain.Enums;

namespace Banking.Domain.Entities;

public class BillPayment
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? AccountId { get; set; }
    public string Payee { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateOnly DueDate { get; set; }
    public BillStatus Status { get; set; }
    public DateTime? PaidAt { get; set; }
}
