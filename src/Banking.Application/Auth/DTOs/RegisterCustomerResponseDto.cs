namespace Banking.Application.Auth.DTOs;

public sealed record RegisterCustomerResponseDto(
    Guid CustomerId,
    string Email,
    string FirstName,
    string LastName);
