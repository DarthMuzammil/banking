namespace Banking.Domain.Entities;

public class CreditCardSpending
{
    public Guid Id { get; set; }
    public Guid CardId { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
