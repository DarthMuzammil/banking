using Banking.Domain.Entities;

namespace Banking.Application.Abstractions;

public interface IJwtTokenService
{
    string GenerateToken(Customer customer);
    string GenerateToken(Customer customer, TimeSpan lifetime);
}
