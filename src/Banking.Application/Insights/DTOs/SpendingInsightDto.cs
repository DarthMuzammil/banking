namespace Banking.Application.Insights.DTOs;

public sealed record SpendingInsightDto(
    string Id,
    string Label,
    decimal Amount,
    decimal ChangePercent,
    string Direction);
