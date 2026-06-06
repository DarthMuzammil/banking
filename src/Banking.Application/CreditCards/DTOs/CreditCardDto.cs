namespace Banking.Application.CreditCards.DTOs;

public sealed record CreditCardDto(
    Guid Id,
    string Name,
    string LastFour,
    decimal Balance,
    decimal Limit,
    DateOnly DueDate,
    decimal MinPayment,
    decimal Apr);

public sealed record CreditCardSpendingDto(
    string Category,
    decimal Amount,
    decimal Percentage);
