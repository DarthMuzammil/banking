using Banking.Application.Abstractions;
using Banking.Application.Auth.DTOs;
using Banking.Application.Common;

namespace Banking.Application.Auth.Commands;

public sealed class RefreshTokenCommandHandler
{
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IJwtTokenService _jwtTokenService;

    public RefreshTokenCommandHandler(
        IRefreshTokenService refreshTokenService,
        IJwtTokenService jwtTokenService)
    {
        _refreshTokenService = refreshTokenService;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<AuthResponseDto>> HandleAsync(
        RefreshTokenCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            return Result<AuthResponseDto>.Failure("Refresh token is required.", "REFRESH_TOKEN_REQUIRED");
        }

        var customer = await _refreshTokenService.ValidateAndRevokeAsync(
            command.RefreshToken.Trim(),
            cancellationToken);

        if (customer is null)
        {
            return Result<AuthResponseDto>.Failure("Invalid or expired refresh token.", "INVALID_REFRESH_TOKEN");
        }

        var accessToken = _jwtTokenService.GenerateToken(customer);
        var refreshToken = await _refreshTokenService.IssueAsync(customer, cancellationToken);

        return Result<AuthResponseDto>.Success(new AuthResponseDto(
            accessToken,
            refreshToken,
            new CustomerSummaryDto(
                customer.Id,
                customer.Email,
                customer.FirstName,
                customer.LastName,
                customer.Role.ToString())));
    }
}
