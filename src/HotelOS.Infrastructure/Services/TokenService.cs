using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HotelOS.Application.Common.Interfaces;
using HotelOS.Domain.Identity.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace HotelOS.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateAccessToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions)
    {
        string secretKey = _configuration["Jwt:SecretKey"] ?? "HotelOS_Super_Secure_Enterprise_Secret_Key_2026_DotNet10_LTS_!#*";
        string issuer = _configuration["Jwt:Issuer"] ?? "HotelOS.Api";
        string audience = _configuration["Jwt:Audience"] ?? "HotelOS.Client";
        int expiryMinutes = int.TryParse(_configuration["Jwt:ExpiryMinutes"], out int exp) ? exp : 60;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("public_id", user.PublicId.ToString()),
            new("first_name", user.FirstName),
            new("last_name", user.LastName)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public (string Token, string TokenHash, DateTimeOffset ExpiresAt) GenerateRefreshToken()
    {
        int days = int.TryParse(_configuration["Jwt:RefreshTokenExpiryDays"], out int d) ? d : 7;
        byte[] randomBytes = RandomNumberGenerator.GetBytes(64);
        string rawToken = Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');

        string tokenHash = HashToken(rawToken);
        DateTimeOffset expiresAt = DateTimeOffset.UtcNow.AddDays(days);

        return (rawToken, tokenHash, expiresAt);
    }

    public (string RawToken, string TokenHash, DateTimeOffset ExpiresAt) GenerateCryptoToken(TimeSpan validFor)
    {
        byte[] randomBytes = RandomNumberGenerator.GetBytes(32);
        string rawToken = Convert.ToHexString(randomBytes).ToLowerInvariant();
        string tokenHash = HashToken(rawToken);
        DateTimeOffset expiresAt = DateTimeOffset.UtcNow.Add(validFor);

        return (rawToken, tokenHash, expiresAt);
    }

    public string HashToken(string rawToken)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(rawToken);
        byte[] hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
