using Banking.Application.Abstractions;
using Banking.Application.Common;
using Banking.Application.Insights.DTOs;
using Banking.Domain.Enums;

namespace Banking.Application.Insights.Queries;

public sealed class GetSpendingInsightsQueryHandler
{
    private readonly ITransactionRepository _transactionRepository;

    public GetSpendingInsightsQueryHandler(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<Result<IReadOnlyList<SpendingInsightDto>>> HandleAsync(
        GetSpendingInsightsQuery query,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var currentMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var nextMonthStart = currentMonthStart.AddMonths(1);
        var previousMonthStart = currentMonthStart.AddMonths(-1);

        var currentRows = await _transactionRepository.GetDebitTotalsByCategoryAsync(
            query.CustomerId, currentMonthStart, nextMonthStart, cancellationToken);

        var previousRows = await _transactionRepository.GetDebitTotalsByCategoryAsync(
            query.CustomerId, previousMonthStart, currentMonthStart, cancellationToken);

        var currentTotal = currentRows.Sum(r => r.Total);
        var previousTotal = previousRows.Sum(r => r.Total);

        var insights = new List<SpendingInsightDto>
        {
            BuildInsight("total", "Total spending", currentTotal, previousTotal)
        };

        foreach (var category in new[]
        {
            TransactionCategory.Bills,
            TransactionCategory.Transfer,
            TransactionCategory.Food,
            TransactionCategory.Shopping
        })
        {
            var current = currentRows.FirstOrDefault(r => r.Category == category)?.Total ?? 0;
            var previous = previousRows.FirstOrDefault(r => r.Category == category)?.Total ?? 0;
            if (current > 0 || previous > 0)
            {
                insights.Add(BuildInsight(
                    category.ToString().ToLowerInvariant(),
                    CategoryLabel(category),
                    current,
                    previous));
            }
        }

        return Result<IReadOnlyList<SpendingInsightDto>>.Success(insights);
    }

    private static SpendingInsightDto BuildInsight(
        string id,
        string label,
        decimal current,
        decimal previous)
    {
        decimal changePercent = 0;
        string direction = "flat";

        if (previous > 0)
        {
            changePercent = Math.Round((current - previous) / previous * 100, 1);
            direction = changePercent > 0 ? "up" : changePercent < 0 ? "down" : "flat";
        }
        else if (current > 0)
        {
            changePercent = 100;
            direction = "up";
        }

        return new SpendingInsightDto(id, label, current, changePercent, direction);
    }

    private static string CategoryLabel(TransactionCategory category) => category switch
    {
        TransactionCategory.Bills => "Bills & utilities",
        TransactionCategory.Transfer => "Transfers",
        TransactionCategory.Food => "Food & dining",
        TransactionCategory.Shopping => "Shopping",
        TransactionCategory.Transport => "Transport",
        TransactionCategory.Income => "Income",
        _ => "Other"
    };
}
