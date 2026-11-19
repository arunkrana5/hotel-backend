using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Common;
using HotelOS.Contracts.Configuration;
using HotelOS.Domain.Configuration.Entities;
using HotelOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelOS.Infrastructure.Services;

public class DocumentNumberingService : IDocumentNumberingService
{
    private readonly HotelOSDbContext _context;
    private readonly ILogger<DocumentNumberingService> _logger;

    public DocumentNumberingService(HotelOSDbContext context, ILogger<DocumentNumberingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<NextDocumentNumberResponse> PreviewNextNumberAsync(
        long propertyId,
        string documentType,
        CancellationToken cancellationToken = default)
    {
        var normalizedType = documentType.Trim().ToUpperInvariant();
        var config = await _context.DocumentNumberConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.PropertyId == propertyId && c.DocumentType == normalizedType && c.IsActive, cancellationToken);

        if (config == null)
        {
            // Fallback default policy if unconfigured
            var fallbackPrefix = normalizedType.Length > 4 ? normalizedType[..3] : normalizedType;
            var fallbackNumber = $"{fallbackPrefix}-{DateTime.UtcNow:yyyyMMdd}-0001";
            return new NextDocumentNumberResponse(normalizedType, fallbackNumber, 1, DateTimeOffset.UtcNow);
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        long nextSeq = config.CurrentSequence + 1;

        if (ShouldReset(config.ResetFrequency, config.LastResetDate, today))
        {
            nextSeq = config.StartingNumber;
        }

        var docNumber = FormatDocumentNumber(config.Prefix, config.DateFormat, config.SequenceLength, nextSeq, config.Suffix);
        return new NextDocumentNumberResponse(normalizedType, docNumber, nextSeq, DateTimeOffset.UtcNow);
    }

    public async Task<NextDocumentNumberResponse> GenerateNextNumberAsync(
        long propertyId,
        string documentType,
        string generatedBy,
        CancellationToken cancellationToken = default)
    {
        var normalizedType = documentType.Trim().ToUpperInvariant();

        // Transactional row update with concurrency safety
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var config = await _context.DocumentNumberConfigurations
            .FirstOrDefaultAsync(c => c.PropertyId == propertyId && c.DocumentType == normalizedType && c.IsActive, cancellationToken);

        if (config == null)
        {
            // Auto-create default config if missing to prevent failure
            var prefix = normalizedType.Length > 4 ? normalizedType[..3] : normalizedType;
            config = new DocumentNumberConfiguration
            {
                PropertyId = propertyId,
                DocumentType = normalizedType,
                Module = "SYSTEM",
                Prefix = prefix,
                DateFormat = "yyyyMMdd",
                SequenceLength = 4,
                StartingNumber = 1,
                ResetFrequency = "DAILY",
                CurrentSequence = 0,
                LastResetDate = DateOnly.FromDateTime(DateTime.UtcNow),
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = generatedBy
            };
            _context.DocumentNumberConfigurations.Add(config);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (ShouldReset(config.ResetFrequency, config.LastResetDate, today))
        {
            config.CurrentSequence = config.StartingNumber;
            config.LastResetDate = today;
        }
        else
        {
            config.CurrentSequence++;
        }

        config.UpdatedAt = DateTimeOffset.UtcNow;
        config.UpdatedBy = generatedBy;

        var docNumber = FormatDocumentNumber(config.Prefix, config.DateFormat, config.SequenceLength, config.CurrentSequence, config.Suffix);

        var audit = new DocumentNumberAudit
        {
            PropertyId = propertyId,
            DocumentType = normalizedType,
            DocumentNumber = docNumber,
            GeneratedBy = generatedBy,
            GeneratedAt = DateTimeOffset.UtcNow,
            Status = "COMMITTED"
        };
        _context.DocumentNumberAudits.Add(audit);

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        _logger.LogInformation("Document number {DocumentNumber} safely generated for {DocumentType} by {User}", docNumber, normalizedType, generatedBy);
        return new NextDocumentNumberResponse(normalizedType, docNumber, config.CurrentSequence, DateTimeOffset.UtcNow);
    }

    public async Task<List<DocumentNumberConfigurationDto>> GetConfigurationsAsync(long propertyId, CancellationToken cancellationToken = default)
    {
        var configs = await _context.DocumentNumberConfigurations
            .AsNoTracking()
            .Where(c => c.PropertyId == propertyId)
            .OrderBy(c => c.Module)
            .ThenBy(c => c.DocumentType)
            .ToListAsync(cancellationToken);

        return configs.Select(c => new DocumentNumberConfigurationDto(
            c.Id,
            c.PropertyId,
            c.DocumentType,
            c.Module,
            c.Prefix,
            c.Suffix,
            c.DateFormat,
            c.SequenceLength,
            c.StartingNumber,
            c.ResetFrequency,
            c.CurrentSequence,
            c.LastResetDate.ToString("yyyy-MM-dd"),
            c.IsActive,
            FormatDocumentNumber(c.Prefix, c.DateFormat, c.SequenceLength, c.CurrentSequence > 0 ? c.CurrentSequence : c.StartingNumber, c.Suffix)
        )).ToList();
    }

    public async Task<DocumentNumberConfigurationDto?> UpdateConfigurationAsync(
        long id,
        UpdateDocumentNumberConfigRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var config = await _context.DocumentNumberConfigurations.FindAsync(new object[] { id }, cancellationToken);
        if (config == null) return null;

        config.Prefix = request.Prefix.Trim().ToUpperInvariant();
        config.Suffix = string.IsNullOrWhiteSpace(request.Suffix) ? null : request.Suffix.Trim().ToUpperInvariant();
        config.DateFormat = request.DateFormat.Trim();
        config.SequenceLength = Math.Clamp(request.SequenceLength, 2, 8);
        config.StartingNumber = Math.Max(1, request.StartingNumber);
        config.ResetFrequency = request.ResetFrequency.Trim().ToUpperInvariant();
        config.IsActive = request.IsActive;
        config.UpdatedAt = DateTimeOffset.UtcNow;
        config.UpdatedBy = updatedBy;

        await _context.SaveChangesAsync(cancellationToken);

        return new DocumentNumberConfigurationDto(
            config.Id,
            config.PropertyId,
            config.DocumentType,
            config.Module,
            config.Prefix,
            config.Suffix,
            config.DateFormat,
            config.SequenceLength,
            config.StartingNumber,
            config.ResetFrequency,
            config.CurrentSequence,
            config.LastResetDate.ToString("yyyy-MM-dd"),
            config.IsActive,
            FormatDocumentNumber(config.Prefix, config.DateFormat, config.SequenceLength, config.CurrentSequence > 0 ? config.CurrentSequence : config.StartingNumber, config.Suffix)
        );
    }

    public async Task<PagedResult<DocumentNumberAuditDto>> GetAuditsPagedAsync(
        long propertyId,
        string? documentType,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.DocumentNumberAudits
            .AsNoTracking()
            .Where(a => a.PropertyId == propertyId);

        if (!string.IsNullOrWhiteSpace(documentType))
        {
            var norm = documentType.Trim().ToUpperInvariant();
            query = query.Where(a => a.DocumentType == norm);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(a => a.GeneratedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(a => new DocumentNumberAuditDto(
            a.Id,
            a.PropertyId,
            a.DocumentType,
            a.DocumentNumber,
            a.GeneratedBy,
            a.GeneratedAt,
            a.EntityId,
            a.EntityType,
            a.Status
        )).ToList();

        return new PagedResult<DocumentNumberAuditDto> { Items = dtos, TotalCount = total, Page = page, PageSize = pageSize };
    }

    private static bool ShouldReset(string frequency, DateOnly lastReset, DateOnly today)
    {
        return frequency.ToUpperInvariant() switch
        {
            "DAILY" => lastReset < today,
            "MONTHLY" => lastReset.Year < today.Year || lastReset.Month < today.Month,
            "YEARLY" => lastReset.Year < today.Year,
            _ => false
        };
    }

    private static string FormatDocumentNumber(string prefix, string dateFormat, int seqLength, long sequence, string? suffix)
    {
        var cleanPrefix = prefix.TrimEnd('-');
        var cleanDateFormat = dateFormat.Trim('-');
        var datePart = "";
        if (!string.IsNullOrWhiteSpace(cleanDateFormat) && cleanDateFormat != "NONE")
        {
            datePart = $"-{DateTime.UtcNow.ToString(cleanDateFormat)}";
        }
        var seqFormat = new string('0', Math.Clamp(seqLength, 2, 8));
        var seqPart = $"-{sequence.ToString(seqFormat)}";
        var suffixPart = string.IsNullOrWhiteSpace(suffix) ? "" : $"-{suffix.Trim('-')}";

        return $"{cleanPrefix}{datePart}{seqPart}{suffixPart}";
    }
}
