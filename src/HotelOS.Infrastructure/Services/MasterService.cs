using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Common;
using HotelOS.Contracts.Masters;
using HotelOS.Domain.Masters.Entities;
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
