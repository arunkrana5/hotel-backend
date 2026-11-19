using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Configuration;
using HotelOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HotelOS.Infrastructure.Services;

public class FormService : IFormService
{
    private readonly HotelOSDbContext _context;

    public FormService(HotelOSDbContext context)
    {
        _context = context;
    }

    public async Task<List<DynamicFormDto>> GetAllFormsAsync(CancellationToken cancellationToken = default)
    {
        var forms = await _context.DynamicFormDefinitions
            .Include(f => f.Fields)
            .AsNoTracking()
            .Where(f => f.IsPublished && !f.IsDeleted)
            .OrderBy(f => f.Title)
            .ToListAsync(cancellationToken);

        return forms.Select(form => new DynamicFormDto(
            form.Id,
            form.Code,
            form.Title,
            form.Description,
            form.Module,
            form.EntityType,
            form.Version,
            form.Fields
                .Where(f => !f.IsDeleted)
                .OrderBy(f => f.DisplayOrder)
                .Select(f => new DynamicFormFieldDto(
                    f.Id,
                    f.FieldKey,
                    f.FieldLabel,
                    f.FieldType,
                    f.IsMandatory,
                    f.DisplayOrder,
                    f.MasterDefinitionCode,
                    f.DefaultValue,
                    f.Placeholder,
                    f.SectionName
                ))
                .ToList()
        )).ToList();
    }

    public async Task<DynamicFormDto?> GetFormByCodeAsync(string formCode, CancellationToken cancellationToken = default)
    {
        string normalized = formCode.Trim().ToUpperInvariant();

        var form = await _context.DynamicFormDefinitions
            .Include(f => f.Fields)
            .AsNoTracking()
            .Where(f => f.Code == normalized && f.IsPublished && !f.IsDeleted)
            .OrderByDescending(f => f.Version)
            .FirstOrDefaultAsync(cancellationToken);

        if (form == null) return null;

        var fields = form.Fields
            .Where(f => !f.IsDeleted)
            .OrderBy(f => f.DisplayOrder)
            .Select(f => new DynamicFormFieldDto(
                f.Id,
                f.FieldKey,
                f.FieldLabel,
                f.FieldType,
                f.IsMandatory,
                f.DisplayOrder,
                f.MasterDefinitionCode,
                f.DefaultValue,
                f.Placeholder,
                f.SectionName
            ))
            .ToList();

        return new DynamicFormDto(
            form.Id,
            form.Code,
            form.Title,
            form.Description,
            form.Module,
            form.EntityType,
            form.Version,
            fields
        );
    }
}
