namespace Banking.Application.Abstractions;

public interface IEmailService
{
    Task SendTransferReceiptAsync(
        string email,
        string reference,
        decimal amount,
        CancellationToken cancellationToken = default);
}
