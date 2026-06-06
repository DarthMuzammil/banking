namespace Banking.Application.Auth.DTOs;

public sealed record AuthResponseDto(
    string Token,
    string RefreshToken,
    CustomerSummaryDto Customer);
