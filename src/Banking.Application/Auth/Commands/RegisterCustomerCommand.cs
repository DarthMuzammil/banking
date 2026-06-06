namespace Banking.Application.Auth.Commands;

public sealed record RegisterCustomerCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName);
