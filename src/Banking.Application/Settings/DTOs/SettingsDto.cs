namespace Banking.Application.Settings.DTOs;

public sealed record SettingsDto(
    string Email,
    string FirstName,
    string LastName,
    bool TwoFactorEnabled,
    NotificationSettingsDto Notifications);

public sealed record NotificationSettingsDto(
    bool Transactions,
    bool Security,
    bool Marketing);

public sealed record UpdateSettingsDto(
    string Email,
    string FirstName,
    string LastName,
    bool TwoFactorEnabled,
    NotificationSettingsDto Notifications);
