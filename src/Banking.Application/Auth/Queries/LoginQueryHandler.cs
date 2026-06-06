using Banking.Application.Abstractions;
using Banking.Application.Auth.DTOs;
using Banking.Application.Common;

namespace Banking.Application.Auth.Queries;

public sealed class LoginQueryHandler
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginQueryHandler(
        ICustomerRepository customerRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _customerRepository = customerRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<AuthResponseDto>> HandleAsync(
        LoginQuery query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query.Email) || string.IsNullOrWhiteSpace(query.Password))
        {
            return Result<AuthResponseDto>.Failure("Email and password are required.", "CREDENTIALS_REQUIRED");
        }

        var normalizedEmail = query.Email.Trim().ToLowerInvariant();
        var customer = await _customerRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (customer is null)
        {
            return Result<AuthResponseDto>.Failure("Invalid email or password.", "INVALID_CREDENTIALS");
        }

        var passwordHash = await _customerRepository.GetPasswordHashAsync(customer.Id, cancellationToken);
        if (passwordHash is null || !_passwordHasher.Verify(query.Password, passwordHash))
        {
            return Result<AuthResponseDto>.Failure("Invalid email or password.", "INVALID_CREDENTIALS");
        }

        var token = _jwtTokenService.GenerateToken(customer);
        var summary = new CustomerSummaryDto(
            customer.Id,
            customer.Email,
            customer.FirstName,
            customer.LastName);

        return Result<AuthResponseDto>.Success(new AuthResponseDto(token, summary));
    }
}
