using Banking.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Banking.Infrastructure.Services;

public sealed class MockEmailService : IEmailService
{
    private readonly ILogger<MockEmailService> _logger;

    public MockEmailService(ILogger<MockEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendTransferReceiptAsync(
        string email,
        string reference,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "EMAIL → {Email}: Transfer {Reference} for {Amount:C} completed.",
            email,
            reference,
            amount);

        return Task.CompletedTask;
    }
}
