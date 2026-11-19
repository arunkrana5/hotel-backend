using System.Security.Claims;
using HotelOS.Application.Common.Interfaces;

namespace HotelOS.Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public long? UserId
    {
        get
        {
            var sub = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue("sub");
            return long.TryParse(sub, out long id) ? id : null;
        }
    }

    public long? PropertyId
    {
        get
        {
            var propClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("property_id");
            return long.TryParse(propClaim, out long id) ? id : 1;
        }
    }

    public string? Username =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name)
        ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue("unique_name");

    public string? Email =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);

    public string IpAddress
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return "127.0.0.1";

            if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded))
            {
                var ip = forwarded.ToString().Split(',')[0].Trim();
                if (!string.IsNullOrEmpty(ip)) return ip;
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        }
    }

    public string UserAgent =>
        _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString() ?? "Unknown";

    public IReadOnlyList<string> Roles =>
        _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
        ?? new List<string>();

    public IReadOnlyList<string> Permissions =>
        _httpContextAccessor.HttpContext?.User?.FindAll("permission").Select(c => c.Value).ToList()
        ?? new List<string>();
}
