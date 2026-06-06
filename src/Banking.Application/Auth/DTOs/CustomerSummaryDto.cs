namespace Banking.Application.Auth.DTOs;

public sealed record CustomerSummaryDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Role);
