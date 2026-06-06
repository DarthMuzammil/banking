using Banking.Application.Abstractions;
using Banking.Application.Common;
using Banking.Application.Settings.DTOs;
using Banking.Domain.Entities;

namespace Banking.Application.Settings.Queries;

public sealed class GetSettingsQueryHandler
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICustomerSettingsRepository _settingsRepository;

    public GetSettingsQueryHandler(
        ICustomerRepository customerRepository,
        ICustomerSettingsRepository settingsRepository)
    {
        _customerRepository = customerRepository;
        _settingsRepository = settingsRepository;
    }

    public async Task<Result<SettingsDto>> HandleAsync(
        GetSettingsQuery query,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(query.CustomerId, cancellationToken);
        if (customer is null)
        {
            return Result<SettingsDto>.Failure("Customer not found.", "CUSTOMER_NOT_FOUND");
        }

        var settings = await _settingsRepository.GetByCustomerIdAsync(query.CustomerId, cancellationToken);
        if (settings is null)
        {
            settings = new CustomerSettings
            {
                CustomerId = query.CustomerId,
                TwoFactorEnabled = false,
                NotifyTransactions = true,
                NotifySecurity = true,
                NotifyMarketing = false,
                UpdatedAt = DateTime.UtcNow
            };
            await _settingsRepository.UpsertAsync(settings, cancellationToken);
        }

        return Result<SettingsDto>.Success(Map(customer, settings));
    }

    internal static SettingsDto Map(Customer customer, CustomerSettings settings) =>
        new(
            customer.Email,
            customer.FirstName,
            customer.LastName,
            settings.TwoFactorEnabled,
            new NotificationSettingsDto(
                settings.NotifyTransactions,
                settings.NotifySecurity,
                settings.NotifyMarketing));
}
