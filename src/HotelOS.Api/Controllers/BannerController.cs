using System.Collections.Concurrent;
using HotelOS.Contracts.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelOS.Api.Controllers;

public record BannerDto(
    long Id,
    string Title,
    string Subtitle,
    string Badge,
    string ImageUrl,
    string CtaText,
    string CtaLink,
    long? PropertyId,
    string? TargetRole,
    bool IsActive,
    int DisplayOrder,
    DateTimeOffset? StartDate,
    DateTimeOffset? EndDate,
    DateTimeOffset CreatedAt
);

public record CreateBannerRequest(
    string Title,
    string Subtitle,
    string? Badge,
    string ImageUrl,
    string? CtaText,
    string? CtaLink,
    long? PropertyId,
    string? TargetRole,
    bool IsActive,
    int DisplayOrder,
    DateTimeOffset? StartDate,
    DateTimeOffset? EndDate
);

[ApiController]
[Route("api/v1/system/banners")]
[Authorize]
public class BannerController : ControllerBase
{
    private static readonly ConcurrentDictionary<long, BannerDto> _banners = new();
    private static long _nextId = 1;

    static BannerController()
    {
        // Seed default high-grade hospitality announcements
        var defaultBanners = new[]
        {
            new BannerDto(
                Id: _nextId++,
                Title: "Grand Luxury Resort & Spa — Operational Excellence",
                Subtitle: "Real-time occupancy yield is trending 4.2% above baseline forecast. 11 rooms ready for allocation.",
                Badge: "Executive Pulse",
                ImageUrl: "https://images.unsplash.com/photo-1566073771259-6a8506099945?auto=format&fit=crop&w=1200&q=80",
                CtaText: "Review Room Matrix",
                CtaLink: "/front-office/room-grid",
                PropertyId: null,
                TargetRole: null,
                IsActive: true,
                DisplayOrder: 1,
                StartDate: DateTimeOffset.UtcNow.AddDays(-1),
                EndDate: DateTimeOffset.UtcNow.AddDays(30),
                CreatedAt: DateTimeOffset.UtcNow
            ),
            new BannerDto(
                Id: _nextId++,
                Title: "Night Audit Settlement & Ledger Reconciliation",
                Subtitle: "City ledger balance is audited and synchronized with double-entry general ledger accounts.",
                Badge: "Finance Nominal",
                ImageUrl: "https://images.unsplash.com/photo-1582719478250-c89cae4dc85b?auto=format&fit=crop&w=1200&q=80",
                CtaText: "Open General Ledger",
                CtaLink: "/finance/gl",
                PropertyId: null,
                TargetRole: null,
                IsActive: true,
                DisplayOrder: 2,
                StartDate: DateTimeOffset.UtcNow.AddDays(-1),
                EndDate: DateTimeOffset.UtcNow.AddDays(30),
                CreatedAt: DateTimeOffset.UtcNow
            ),
            new BannerDto(
                Id: _nextId++,
                Title: "VIP Arrivals & Concierge Protocols Active",
                Subtitle: "2 VIP guest check-ins scheduled today. Concierge welcome suites inspected and sanitized.",
                Badge: "Front Desk Alert",
                ImageUrl: "https://images.unsplash.com/photo-1542314831-068cd1dbfeeb?auto=format&fit=crop&w=1200&q=80",
                CtaText: "Inspect Arrivals Queue",
                CtaLink: "/front-office/arrivals",
                PropertyId: null,
                TargetRole: null,
                IsActive: true,
                DisplayOrder: 3,
                StartDate: DateTimeOffset.UtcNow.AddDays(-1),
                EndDate: DateTimeOffset.UtcNow.AddDays(30),
                CreatedAt: DateTimeOffset.UtcNow
            )
        };

        foreach (var b in defaultBanners)
        {
            _banners[b.Id] = b;
        }
    }

    [HttpGet]
    public ActionResult<ApiResponse<List<BannerDto>>> GetBanners([FromQuery] long? propertyId = null, [FromQuery] bool activeOnly = true)
    {
        var now = DateTimeOffset.UtcNow;
        var query = _banners.Values.AsEnumerable();

        if (activeOnly)
        {
            query = query.Where(b => b.IsActive &&
                (!b.StartDate.HasValue || b.StartDate <= now) &&
                (!b.EndDate.HasValue || b.EndDate >= now));
        }

        if (propertyId.HasValue && propertyId.Value > 0)
        {
            query = query.Where(b => !b.PropertyId.HasValue || b.PropertyId == propertyId.Value);
        }

        var list = query.OrderBy(b => b.DisplayOrder).ToList();
        return Ok(ApiResponse<List<BannerDto>>.Ok(list));
    }

    [HttpPost]
    public ActionResult<ApiResponse<BannerDto>> CreateBanner([FromBody] CreateBannerRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Title) || string.IsNullOrWhiteSpace(req.ImageUrl))
        {
            return BadRequest(ApiResponse<BannerDto>.Fail("Title and Image URL are required."));
        }

        long id = Interlocked.Increment(ref _nextId);
        var banner = new BannerDto(
            Id: id,
            Title: req.Title.Trim(),
            Subtitle: req.Subtitle?.Trim() ?? string.Empty,
            Badge: req.Badge?.Trim() ?? "Announcement",
            ImageUrl: req.ImageUrl.Trim(),
            CtaText: req.CtaText?.Trim() ?? "View Details",
            CtaLink: req.CtaLink?.Trim() ?? "/dashboard",
            PropertyId: req.PropertyId,
            TargetRole: req.TargetRole,
            IsActive: req.IsActive,
            DisplayOrder: req.DisplayOrder,
            StartDate: req.StartDate,
            EndDate: req.EndDate,
            CreatedAt: DateTimeOffset.UtcNow
        );

        _banners[id] = banner;
        return Ok(ApiResponse<BannerDto>.Ok(banner, "Banner created successfully"));
    }

    [HttpPut("{id:long}")]
    public ActionResult<ApiResponse<BannerDto>> UpdateBanner(long id, [FromBody] CreateBannerRequest req)
    {
        if (!_banners.TryGetValue(id, out var existing))
        {
            return NotFound(ApiResponse<BannerDto>.Fail("Banner not found"));
        }

        var updated = existing with
        {
            Title = req.Title.Trim(),
            Subtitle = req.Subtitle?.Trim() ?? string.Empty,
            Badge = req.Badge?.Trim() ?? "Announcement",
            ImageUrl = req.ImageUrl.Trim(),
            CtaText = req.CtaText?.Trim() ?? "View Details",
            CtaLink = req.CtaLink?.Trim() ?? "/dashboard",
            PropertyId = req.PropertyId,
            TargetRole = req.TargetRole,
            IsActive = req.IsActive,
            DisplayOrder = req.DisplayOrder,
            StartDate = req.StartDate,
            EndDate = req.EndDate
        };

        _banners[id] = updated;
        return Ok(ApiResponse<BannerDto>.Ok(updated, "Banner updated successfully"));
    }

    [HttpDelete("{id:long}")]
    public ActionResult<ApiResponse<bool>> DeleteBanner(long id)
    {
        if (_banners.TryRemove(id, out _))
        {
            return Ok(ApiResponse<bool>.Ok(true, "Banner deleted successfully"));
        }
        return NotFound(ApiResponse<bool>.Fail("Banner not found"));
    }
}
