namespace Banking.Application.Investments.DTOs;

public sealed record PortfolioDto(
    decimal TotalValue,
    decimal DayChange,
    decimal DayChangePercent,
    IReadOnlyList<HoldingDto> Holdings);

public sealed record HoldingDto(
    Guid Id,
    string Symbol,
    string Name,
    decimal Shares,
    decimal Value,
    decimal ChangePercent);
