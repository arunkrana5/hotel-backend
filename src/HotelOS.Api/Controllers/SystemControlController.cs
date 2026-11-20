using System.Diagnostics;
using HotelOS.Contracts.Common;
using HotelOS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelOS.Api.Controllers;

public record SystemHealthDto(
    string Status,
    bool DatabaseConnected,
    string DatabaseEngine,
    string HostEnvironment,
    string ProcessUptime,
    long MemoryWorkingSetMb,
    string ServerTimeUtc,
    int TotalUsers,
    int TotalRooms,
    int TotalReservations
);

public record AuditLogItemDto(
    long Id,
    string Action,
    string EntityName,
    string? EntityId,
    string? PerformedBy,
    string? IpAddress,
    string? ChangesJson,
    DateTimeOffset Timestamp
);

public record SecurityEventItemDto(
    long Id,
    string EventType,
    string Description,
    string? IpAddress,
    string? UserAgent,
    string? Username,
    DateTimeOffset OccurredAt
);

public record UpdateMenuRequest(string Title, int DisplayOrder, bool IsActive, string? BadgeText);

[ApiController]
[Route("api/v1/system")]
[Authorize]
public class SystemControlController : ControllerBase
{
    private readonly HotelOSDbContext _context;

    public SystemControlController(HotelOSDbContext context)
    {
        _context = context;
    }

    [HttpGet("health")]
    public async Task<IActionResult> GetSystemHealth(CancellationToken cancellationToken)
    {
        bool dbConnected = false;
        int userCount = 0;
        int roomCount = 0;
        int resCount = 0;

        try
        {
            dbConnected = await _context.Database.CanConnectAsync(cancellationToken);
            if (dbConnected)
            {
                userCount = await _context.Users.CountAsync(cancellationToken);
                roomCount = await _context.Rooms.CountAsync(cancellationToken);
                resCount = await _context.Reservations.CountAsync(cancellationToken);
            }
        }
        catch
        {
            dbConnected = false;
        }

        var process = Process.GetCurrentProcess();
        var uptime = DateTime.UtcNow - process.StartTime.ToUniversalTime();

        var health = new SystemHealthDto(
            Status: dbConnected ? "HEALTHY" : "DEGRADED",
            DatabaseConnected: dbConnected,
            DatabaseEngine: "PostgreSQL 17.x with Npgsql EF Core 10",
            HostEnvironment: Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            ProcessUptime: $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s",
            MemoryWorkingSetMb: process.WorkingSet64 / (1024 * 1024),
            ServerTimeUtc: DateTimeOffset.UtcNow.ToString("O"),
            TotalUsers: userCount,
            TotalRooms: roomCount,
            TotalReservations: resCount
        );

        return Ok(ApiResponse<SystemHealthDto>.Ok(health));
    }

    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 15,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            string s = search.Trim().ToLowerInvariant();
            query = query.Where(a =>
                a.Action.ToLower().Contains(s) ||
                a.EntityName.ToLower().Contains(s) ||
                (a.PerformedBy != null && a.PerformedBy.ToLower().Contains(s)));
        }

        int total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.PerformedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuditLogItemDto(
                a.Id,
                a.Action,
                a.EntityName,
                a.EntityId,
                a.PerformedBy,
                a.IpAddress,
                a.ChangesJson,
                a.PerformedAt
            ))
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<PagedResult<AuditLogItemDto>>.Ok(
            PagedResult<AuditLogItemDto>.Create(items, total, page, pageSize)));
    }

    [HttpGet("security-events")]
    public async Task<IActionResult> GetSecurityEvents(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 15,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.SecurityEvents
            .AsNoTracking()
            .Include(e => e.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            string s = search.Trim().ToLowerInvariant();
            query = query.Where(e =>
                e.Description.ToLower().Contains(s) ||
                (e.User != null && e.User.Username.ToLower().Contains(s)));
        }

        int total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(e => e.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new SecurityEventItemDto(
                e.Id,
                e.EventType.ToString(),
                e.Description,
                e.IpAddress,
                e.UserAgent,
                e.User != null ? e.User.Username : null,
                e.OccurredAt
            ))
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<PagedResult<SecurityEventItemDto>>.Ok(
            PagedResult<SecurityEventItemDto>.Create(items, total, page, pageSize)));
    }

    [HttpGet("menus")]
    public async Task<IActionResult> GetAllMenus(CancellationToken cancellationToken)
    {
        var allMenus = await _context.NavigationMenus
            .AsNoTracking()
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<List<HotelOS.Domain.Configuration.Entities.NavigationMenu>>.Ok(allMenus));
    }

    [HttpPut("menus/{id}")]
    public async Task<IActionResult> UpdateMenu(long id, [FromBody] UpdateMenuRequest request, CancellationToken cancellationToken)
    {
        var menu = await _context.NavigationMenus.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        if (menu == null)
            return NotFound(ApiResponse<bool>.Fail("Navigation menu not found."));

        menu.Title = request.Title;
        menu.DisplayOrder = request.DisplayOrder;
        menu.IsActive = request.IsActive;
        menu.BadgeText = request.BadgeText;
        menu.ModifiedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Ok(ApiResponse<bool>.Ok(true, "Menu configuration updated."));
    }
}
