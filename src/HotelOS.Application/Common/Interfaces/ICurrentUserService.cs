namespace HotelOS.Application.Common.Interfaces;

public interface ICurrentUserService
{
    long? UserId { get; }
    long? PropertyId { get; }
    string? Username { get; }
    string? Email { get; }
    string IpAddress { get; }
    string UserAgent { get; }
    IReadOnlyList<string> Roles { get; }
    IReadOnlyList<string> Permissions { get; }
}
