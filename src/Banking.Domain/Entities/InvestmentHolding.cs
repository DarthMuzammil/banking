namespace Banking.Domain.Entities;

public class InvestmentHolding
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Shares { get; set; }
    public decimal CurrentValue { get; set; }
    public decimal DayChangePercent { get; set; }
}
