using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Banking.Application.Abstractions;
using Banking.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Banking.Infrastructure.Security;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;

    public JwtTokenService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    public string GenerateToken(Customer customer) =>
        GenerateToken(customer, TimeSpan.FromMinutes(_settings.ExpiryMinutes));

    public string GenerateToken(Customer customer, TimeSpan lifetime)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, customer.Id.ToString()),
            new(ClaimTypes.NameIdentifier, customer.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, customer.Email),
            new(JwtRegisteredClaimNames.GivenName, customer.FirstName),
            new(JwtRegisteredClaimNames.FamilyName, customer.LastName),
            new(ClaimTypes.Role, customer.Role.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.Add(lifetime),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
