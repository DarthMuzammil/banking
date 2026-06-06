namespace Banking.Domain.Exceptions;

public sealed class InsufficientFundsException : Exception
{
    public Guid AccountId { get; }
    public decimal RequestedAmount { get; }
    public decimal AvailableBalance { get; }

    public InsufficientFundsException(Guid accountId, decimal requestedAmount, decimal availableBalance)
        : base($"Account {accountId} has insufficient funds. Requested {requestedAmount}, available {availableBalance}.")
    {
        AccountId = accountId;
        RequestedAmount = requestedAmount;
        AvailableBalance = availableBalance;
    }
}
