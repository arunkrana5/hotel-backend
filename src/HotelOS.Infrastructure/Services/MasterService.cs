using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Common;
using HotelOS.Contracts.Masters;
using HotelOS.Domain.Masters.Entities;
using HotelOS.Domain.Pms.Entities;
using HotelOS.Domain.Purchase.Entities;
using HotelOS.Domain.Finance.Entities;
using HotelOS.Domain.Hr.Entities;
using HotelOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HotelOS.Infrastructure.Services;

public class MasterService : IMasterService
{
    private readonly HotelOSDbContext _context;

    public MasterService(HotelOSDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<DropdownOptionDto>> GetDropdownOptionsAsync(
        string masterCode,
        int page,
        int pageSize,
        string? search,
        long? propertyId = null,
        long? parentValueId = null,
        CancellationToken cancellationToken = default)
    {
        // Enforce Section 8 Rule: Initial load & batch size is capped at 5 records!
        if (pageSize > 5 || pageSize <= 0)
        {
            pageSize = 5;
        }

        if (page <= 0) page = 1;

        string normalizedCode = masterCode.Trim().ToUpperInvariant();

        // Mode A - Special Entity Lookups with Strict 5-record pagination & server search
        if (normalizedCode == "PROPERTY")
        {
            var props = new List<DropdownOptionDto>
            {
                new(1, "PROP-01", "Grand Luxury Resort & Spa", "Flagship 5-Star Property", null, null, true)
            };
            if (!string.IsNullOrWhiteSpace(search))
            {
                props = props.Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) || p.Code.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            return PagedResult<DropdownOptionDto>.Create(props, props.Count, 1, pageSize);
        }

        if (normalizedCode == "ROOM")
        {
            var rq = _context.Rooms.AsNoTracking().Where(r => !r.IsDeleted);
            if (propertyId.HasValue) rq = rq.Where(r => r.PropertyId == propertyId.Value);
            if (parentValueId.HasValue) rq = rq.Where(r => r.FloorNumber == parentValueId.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.Trim().ToLowerInvariant();
                rq = rq.Where(r => r.RoomNumber.ToLower().Contains(s) || r.RoomTypeCode.ToLower().Contains(s));
            }
            int total = await rq.CountAsync(cancellationToken);
            var list = await rq.OrderBy(r => r.RoomNumber)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new DropdownOptionDto(r.Id, r.RoomNumber, $"Room {r.RoomNumber} ({r.RoomTypeCode})", $"Floor {r.FloorNumber} • {r.RoomStatusCode} • ${r.BaseRatePerNight}", r.BaseRatePerNight, r.FloorNumber, false))
                .ToListAsync(cancellationToken);
            return PagedResult<DropdownOptionDto>.Create(list, total, page, pageSize);
        }

        if (normalizedCode == "GUEST" || normalizedCode == "CUSTOMER")
        {
            var gq = _context.Guests.AsNoTracking().Where(g => !g.IsDeleted);
            if (propertyId.HasValue) gq = gq.Where(g => g.PropertyId == propertyId.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.Trim().ToLowerInvariant();
                gq = gq.Where(g => g.FirstName.ToLower().Contains(s) || g.LastName.ToLower().Contains(s) || g.Email.ToLower().Contains(s) || g.Phone.Contains(s));
            }
            int total = await gq.CountAsync(cancellationToken);
            var list = await gq.OrderBy(g => g.LastName).ThenBy(g => g.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(g => new DropdownOptionDto(g.Id, g.Email, $"{g.FirstName} {g.LastName}", $"{g.VipStatus} VIP • {g.Phone}", g.LifetimeSpend, null, false))
                .ToListAsync(cancellationToken);
            return PagedResult<DropdownOptionDto>.Create(list, total, page, pageSize);
        }

        if (normalizedCode == "VENDOR")
        {
            var vq = _context.Vendors.AsNoTracking().Where(v => !v.IsDeleted && v.Status == "ACTIVE");
            if (propertyId.HasValue) vq = vq.Where(v => v.PropertyId == propertyId.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.Trim().ToLowerInvariant();
                vq = vq.Where(v => v.VendorName.ToLower().Contains(s) || v.VendorCode.ToLower().Contains(s) || v.ContactPerson.ToLower().Contains(s));
            }
            int total = await vq.CountAsync(cancellationToken);
            var list = await vq.OrderBy(v => v.VendorName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(v => new DropdownOptionDto(v.Id, v.VendorCode, v.VendorName, $"{v.ContactPerson} • {v.Phone}", v.PaymentTermsDays, null, false))
                .ToListAsync(cancellationToken);
            return PagedResult<DropdownOptionDto>.Create(list, total, page, pageSize);
        }

        if (normalizedCode == "ACCOUNT")
        {
            var aq = _context.Accounts.AsNoTracking().Where(a => !a.IsDeleted);
            if (propertyId.HasValue) aq = aq.Where(a => a.PropertyId == propertyId.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.Trim().ToLowerInvariant();
                aq = aq.Where(a => a.AccountName.ToLower().Contains(s) || a.AccountCode.ToLower().Contains(s) || a.AccountType.ToLower().Contains(s));
            }
            int total = await aq.CountAsync(cancellationToken);
            var list = await aq.OrderBy(a => a.AccountCode)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new DropdownOptionDto(a.Id, a.AccountCode, $"{a.AccountCode} - {a.AccountName}", $"{a.AccountType} • Balance: ${a.Balance}", a.Balance, null, false))
                .ToListAsync(cancellationToken);
            return PagedResult<DropdownOptionDto>.Create(list, total, page, pageSize);
        }

        if (normalizedCode == "EMPLOYEE")
        {
            var eq = _context.Employees.AsNoTracking().Where(e => !e.IsDeleted && e.Status == "ACTIVE");
            if (propertyId.HasValue) eq = eq.Where(e => e.PropertyId == propertyId.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.Trim().ToLowerInvariant();
                eq = eq.Where(e => e.FirstName.ToLower().Contains(s) || e.LastName.ToLower().Contains(s) || e.EmployeeCode.ToLower().Contains(s) || e.Designation.ToLower().Contains(s));
            }
            int total = await eq.CountAsync(cancellationToken);
            var list = await eq.OrderBy(e => e.LastName).ThenBy(e => e.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new DropdownOptionDto(e.Id, e.EmployeeCode, $"{e.FirstName} {e.LastName}", $"{e.Designation} • Dept: {e.DepartmentCode}", e.BasicSalary, null, false))
                .ToListAsync(cancellationToken);
            return PagedResult<DropdownOptionDto>.Create(list, total, page, pageSize);
        }

        if (normalizedCode == "PAYMENT_METHOD")
        {
            normalizedCode = "PAYMENT_MODE";
        }

        var query = _context.MasterValues
            .AsNoTracking()
            .Where(v => v.MasterDefinition!.Code == normalizedCode && v.IsActive && !v.IsDeleted);

        if (propertyId.HasValue)
        {
            query = query.Where(v => v.PropertyId == null || v.PropertyId == propertyId.Value);
        }

        if (parentValueId.HasValue)
        {
            query = query.Where(v => v.ParentValueId == parentValueId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            string searchPattern = search.Trim().ToLowerInvariant();
            query = query.Where(v => v.Name.ToLower().Contains(searchPattern) || v.Code.ToLower().Contains(searchPattern));
        }

        int totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(v => v.DisplayOrder)
            .ThenBy(v => v.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => new DropdownOptionDto(
                v.Id,
                v.Code,
                v.Name,
                v.Description,
                v.NumericValue,
                v.ParentValueId,
                v.IsDefault
            ))
            .ToListAsync(cancellationToken);

        return PagedResult<DropdownOptionDto>.Create(items, totalCount, page, pageSize);
    }

    public async Task<PagedResult<MasterValueDto>> GetMasterValuesPagedAsync(
        string masterCode,
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default)
    {
        // Default 10 for table, supported: 10, 20, 50, 100
        if (pageSize <= 0) pageSize = 10;
        if (page <= 0) page = 1;

        string normalizedCode = masterCode.Trim().ToUpperInvariant();

        var query = _context.MasterValues
            .Include(v => v.MasterDefinition)
            .AsNoTracking()
            .Where(v => v.MasterDefinition!.Code == normalizedCode && !v.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            string searchPattern = search.Trim().ToLowerInvariant();
            query = query.Where(v => v.Name.ToLower().Contains(searchPattern) || v.Code.ToLower().Contains(searchPattern));
        }

        int totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(v => v.DisplayOrder)
            .ThenBy(v => v.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => new MasterValueDto(
                v.Id,
                v.MasterDefinitionId,
                v.MasterDefinition!.Code,
                v.PropertyId,
                v.ParentValueId,
                v.Code,
                v.Name,
                v.Description,
                v.NumericValue,
                v.ExtraDataJson,
                v.DisplayOrder,
                v.IsActive,
                v.IsDefault
            ))
            .ToListAsync(cancellationToken);

        return PagedResult<MasterValueDto>.Create(items, totalCount, page, pageSize);
    }

    public async Task<ApiResponse<MasterValueDto>> CreateMasterValueAsync(
        CreateMasterValueRequest request,
        string username,
        CancellationToken cancellationToken = default)
    {
        var def = await _context.MasterDefinitions.FirstOrDefaultAsync(d => d.Id == request.MasterDefinitionId, cancellationToken);
        if (def == null)
            return ApiResponse<MasterValueDto>.Fail("Master definition not found.", 404);

        string normalizedCode = request.Code.Trim().ToUpperInvariant();
        bool exists = await _context.MasterValues.AnyAsync(
            v => v.MasterDefinitionId == request.MasterDefinitionId && v.Code == normalizedCode && !v.IsDeleted,
            cancellationToken);

        if (exists)
            return ApiResponse<MasterValueDto>.Fail($"Master value code '{normalizedCode}' already exists in this definition.", 400);

        var entity = new MasterValue
        {
            MasterDefinitionId = request.MasterDefinitionId,
            PropertyId = request.PropertyId,
            ParentValueId = request.ParentValueId,
            Code = normalizedCode,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            NumericValue = request.NumericValue,
            ExtraDataJson = request.ExtraDataJson,
            DisplayOrder = request.DisplayOrder,
            IsDefault = request.IsDefault,
            IsActive = true,
            CreatedBy = username
        };

        _context.MasterValues.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new MasterValueDto(
            entity.Id,
            entity.MasterDefinitionId,
            def.Code,
            entity.PropertyId,
            entity.ParentValueId,
            entity.Code,
            entity.Name,
            entity.Description,
            entity.NumericValue,
            entity.ExtraDataJson,
            entity.DisplayOrder,
            entity.IsActive,
            entity.IsDefault);

        return ApiResponse<MasterValueDto>.Ok(dto, "Master value created successfully.");
    }

    public async Task<ApiResponse<MasterValueDto>> UpdateMasterValueAsync(
        long id,
        UpdateMasterValueRequest request,
        string username,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.MasterValues
            .Include(v => v.MasterDefinition)
            .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted, cancellationToken);

        if (entity == null)
            return ApiResponse<MasterValueDto>.Fail("Master value not found.", 404);

        entity.Name = request.Name.Trim();
        entity.Description = request.Description?.Trim() ?? string.Empty;
        entity.ParentValueId = request.ParentValueId;
        entity.NumericValue = request.NumericValue;
        entity.ExtraDataJson = request.ExtraDataJson;
        entity.DisplayOrder = request.DisplayOrder;
        entity.IsActive = request.IsActive;
        entity.IsDefault = request.IsDefault;
        entity.ModifiedAt = DateTimeOffset.UtcNow;
        entity.ModifiedBy = username;

        await _context.SaveChangesAsync(cancellationToken);

        var dto = new MasterValueDto(
            entity.Id,
            entity.MasterDefinitionId,
            entity.MasterDefinition!.Code,
            entity.PropertyId,
            entity.ParentValueId,
            entity.Code,
            entity.Name,
            entity.Description,
            entity.NumericValue,
            entity.ExtraDataJson,
            entity.DisplayOrder,
            entity.IsActive,
            entity.IsDefault);

        return ApiResponse<MasterValueDto>.Ok(dto, "Master value updated successfully.");
    }

    public async Task<ApiResponse> DeleteMasterValueAsync(
        long id,
        string username,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.MasterValues.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
        if (entity == null)
            return ApiResponse.FailMessage("Master value not found.", 404);

        // Soft delete
        entity.IsDeleted = true;
        entity.DeletedAt = DateTimeOffset.UtcNow;
        entity.DeletedBy = username;

        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponse.OkMessage("Master value deleted successfully.");
    }

    public async Task<List<MasterDefinitionDto>> GetAllDefinitionsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.MasterDefinitions
            .Include(d => d.Category)
            .AsNoTracking()
            .OrderBy(d => d.DisplayOrder)
            .ThenBy(d => d.Name)
            .Select(d => new MasterDefinitionDto(
                d.Id,
                d.Code,
                d.Name,
                d.Category != null ? d.Category.Code : string.Empty,
                d.Description,
                d.IsPropertySpecific,
                d.IsHierarchical,
                d.DisplayOrder
            ))
            .ToListAsync(cancellationToken);
    }
}
