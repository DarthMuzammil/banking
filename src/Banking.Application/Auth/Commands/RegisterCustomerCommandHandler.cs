using Banking.Application.Abstractions;
using Banking.Application.Auth.DTOs;
using Banking.Application.Common;
using Banking.Domain.Entities;

namespace Banking.Application.Auth.Commands;

public sealed class RegisterCustomerCommandHandler
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterCustomerCommandHandler(
        ICustomerRepository customerRepository,
        IPasswordHasher passwordHasher)
    {
        _customerRepository = customerRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<RegisterCustomerResponseDto>> HandleAsync(
        RegisterCustomerCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Email))
        {
            return Result<RegisterCustomerResponseDto>.Failure("Email is required.", "EMAIL_REQUIRED");
        }

        if (string.IsNullOrWhiteSpace(command.Password) || command.Password.Length < 8)
        {
            return Result<RegisterCustomerResponseDto>.Failure(
                "Password must be at least 8 characters.",
                "PASSWORD_TOO_SHORT");
        }

        if (string.IsNullOrWhiteSpace(command.FirstName) || string.IsNullOrWhiteSpace(command.LastName))
        {
            return Result<RegisterCustomerResponseDto>.Failure(
                "First name and last name are required.",
                "NAME_REQUIRED");
        }

        var normalizedEmail = command.Email.Trim().ToLowerInvariant();
        var existingCustomer = await _customerRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (existingCustomer is not null)
        {
            return Result<RegisterCustomerResponseDto>.Failure(
                "A customer with this email already exists.",
                "EMAIL_ALREADY_EXISTS");
        }

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            FirstName = command.FirstName.Trim(),
            LastName = command.LastName.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        var passwordHash = _passwordHasher.Hash(command.Password);
        var customerId = await _customerRepository.CreateAsync(customer, passwordHash, cancellationToken);

        return Result<RegisterCustomerResponseDto>.Success(new RegisterCustomerResponseDto(
            customerId,
            customer.Email,
            customer.FirstName,
            customer.LastName));
    }
}
