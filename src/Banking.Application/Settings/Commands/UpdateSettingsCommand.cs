using Banking.Application.Settings.DTOs;

namespace Banking.Application.Settings.Commands;

public sealed record UpdateSettingsCommand(Guid CustomerId, UpdateSettingsDto Settings);
