using Banking.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Banking.Infrastructure.Services;

public sealed class MockPaymentGatewayService : IPaymentGatewayService
{
    private readonly ILogger<MockPaymentGatewayService> _logger;

    public MockPaymentGatewayService(ILogger<MockPaymentGatewayService> logger)
    {
        _logger = logger;
    }

    public Task<string> ProcessBillPaymentAsync(
        string payee,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        var reference = $"BILL-{DateTime.UtcNow:yyyyMMddHHmmss}";
        _logger.LogInformation(
            "PAYMENT GATEWAY → Paid {Payee} {Amount:C}. Reference: {Reference}",
            payee,
            amount,
            reference);

        return Task.FromResult(reference);
    }
}
