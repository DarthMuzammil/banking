using Banking.Application.Abstractions;
using Banking.Application.Payments.DTOs;
using Banking.Domain.Enums;

namespace Banking.Application.Payments.Queries;

public sealed class GetBillPaymentsQueryHandler
{
    private readonly IBillPaymentRepository _billPaymentRepository;

    public GetBillPaymentsQueryHandler(IBillPaymentRepository billPaymentRepository)
    {
        _billPaymentRepository = billPaymentRepository;
    }

    public async Task<IReadOnlyList<BillPaymentDto>> HandleAsync(
        GetBillPaymentsQuery query,
        CancellationToken cancellationToken = default)
    {
        var bills = await _billPaymentRepository.GetByCustomerIdAsync(query.CustomerId, cancellationToken);

        return bills
            .Select(b => new BillPaymentDto(
                b.Id,
                b.Payee,
                b.Amount,
                b.DueDate,
                MapStatus(b.Status),
                b.AccountId))
            .ToList();
    }

    private static string MapStatus(BillStatus status) => status switch
    {
        BillStatus.Due => "Due",
        BillStatus.Paid => "Paid",
        BillStatus.Scheduled => "Scheduled",
        _ => "Due"
    };
}
