namespace Banking.Application.Abstractions;

public interface IPaymentGatewayService
{
    Task<string> ProcessBillPaymentAsync(
        string payee,
        decimal amount,
        CancellationToken cancellationToken = default);
}
