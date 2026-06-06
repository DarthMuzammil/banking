using Banking.Application.Abstractions;
using Banking.Application.Payments.DTOs;
using Banking.Domain.Enums;

namespace Banking.Application.Payments.Queries;

public sealed class GetScheduledPaymentsQueryHandler
{
    private readonly IScheduledPaymentRepository _scheduledPaymentRepository;

    public GetScheduledPaymentsQueryHandler(IScheduledPaymentRepository scheduledPaymentRepository)
    {
        _scheduledPaymentRepository = scheduledPaymentRepository;
    }

    public async Task<IReadOnlyList<ScheduledPaymentDto>> HandleAsync(
        GetScheduledPaymentsQuery query,
        CancellationToken cancellationToken = default)
    {
        var items = await _scheduledPaymentRepository.GetByCustomerIdAsync(query.CustomerId, cancellationToken);

        return items
            .Select(s => new ScheduledPaymentDto(
                s.Id,
                s.Payee,
                s.Amount,
                MapFrequency(s.Frequency),
                s.NextDate,
                s.AccountId))
            .ToList();
    }

    private static string MapFrequency(PaymentFrequency frequency) => frequency switch
    {
        PaymentFrequency.Weekly => "Weekly",
        PaymentFrequency.Monthly => "Monthly",
        PaymentFrequency.Quarterly => "Quarterly",
        _ => "Monthly"
    };
}
