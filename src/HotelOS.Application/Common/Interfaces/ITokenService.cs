using HotelOS.Domain.Identity.Entities;

namespace HotelOS.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions);
    (string Token, string TokenHash, DateTimeOffset ExpiresAt) GenerateRefreshToken();
    (string RawToken, string TokenHash, DateTimeOffset ExpiresAt) GenerateCryptoToken(TimeSpan validFor);
    string HashToken(string rawToken);
}
