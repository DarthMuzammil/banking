using Banking.Application.Abstractions;
using Banking.Application.Common;
using Banking.Application.Settings.DTOs;
using Banking.Application.Settings.Queries;
using Banking.Domain.Entities;

namespace Banking.Application.Settings.Commands;

public sealed class UpdateSettingsCommandHandler
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICustomerSettingsRepository _settingsRepository;

    public UpdateSettingsCommandHandler(
        ICustomerRepository customerRepository,
        ICustomerSettingsRepository settingsRepository)
    {
        _customerRepository = customerRepository;
        _settingsRepository = settingsRepository;
    }

    public async Task<Result<SettingsDto>> HandleAsync(
        UpdateSettingsCommand command,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(command.CustomerId, cancellationToken);
        if (customer is null)
        {
            return Result<SettingsDto>.Failure("Customer not found.", "CUSTOMER_NOT_FOUND");
        }

        await _customerRepository.UpdateProfileAsync(
            command.CustomerId,
            command.Settings.FirstName.Trim(),
            command.Settings.LastName.Trim(),
            command.Settings.Email.Trim(),
            cancellationToken);

        var settings = new CustomerSettings
        {
            CustomerId = command.CustomerId,
            TwoFactorEnabled = command.Settings.TwoFactorEnabled,
            NotifyTransactions = command.Settings.Notifications.Transactions,
            NotifySecurity = command.Settings.Notifications.Security,
            NotifyMarketing = command.Settings.Notifications.Marketing,
            UpdatedAt = DateTime.UtcNow
        };

        await _settingsRepository.UpsertAsync(settings, cancellationToken);

        customer.FirstName = command.Settings.FirstName.Trim();
        customer.LastName = command.Settings.LastName.Trim();
        customer.Email = command.Settings.Email.Trim();

        return Result<SettingsDto>.Success(GetSettingsQueryHandler.Map(customer, settings));
    }
}
