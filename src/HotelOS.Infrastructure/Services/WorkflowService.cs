using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Workflow;
using HotelOS.Domain.Workflow.Entities;
using HotelOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HotelOS.Infrastructure.Services;

public class WorkflowService : IWorkflowService
{
    private readonly HotelOSDbContext _context;

    public WorkflowService(HotelOSDbContext context)
    {
        _context = context;
    }

    public async Task<List<WorkflowDefinitionDto>> GetAllDefinitionsAsync(CancellationToken cancellationToken = default)
    {
        var definitions = await _context.WorkflowDefinitions
            .Include(w => w.Steps)
            .Where(w => w.IsPublished)
            .OrderBy(w => w.Title)
            .ToListAsync(cancellationToken);

        return definitions.Select(d => new WorkflowDefinitionDto(
            d.Id,
            d.Code,
            d.Title,
            d.Module,
            d.Version,
            d.Description,
            d.Steps.OrderBy(s => s.StepOrder)
                .Select(s => new WorkflowStepDto(s.Id, s.StepOrder, s.StepName, s.StepType, s.AssignedRole, s.SlaMinutes))
                .ToList()
        )).ToList();
    }

    public async Task<WorkflowDefinitionDto?> GetDefinitionAsync(string code, CancellationToken cancellationToken = default)
    {
        string normalizedCode = code.Trim().ToUpperInvariant();
        var d = await _context.WorkflowDefinitions
            .Include(w => w.Steps)
            .FirstOrDefaultAsync(w => w.Code == normalizedCode && w.IsPublished, cancellationToken);

        if (d == null) return null;

        return new WorkflowDefinitionDto(
            d.Id,
            d.Code,
            d.Title,
            d.Module,
            d.Version,
            d.Description,
            d.Steps.OrderBy(s => s.StepOrder)
                .Select(s => new WorkflowStepDto(s.Id, s.StepOrder, s.StepName, s.StepType, s.AssignedRole, s.SlaMinutes))
                .ToList()
        );
    }

    public async Task<WorkflowInstanceDto> StartWorkflowAsync(
        string workflowCode,
        string entityType,
        long entityId,
        long userId,
        CancellationToken cancellationToken = default)
    {
        string normalizedCode = workflowCode.Trim().ToUpperInvariant();

        var definition = await _context.WorkflowDefinitions
            .Include(w => w.Steps)
            .Include(w => w.Transitions)
            .FirstOrDefaultAsync(w => w.Code == normalizedCode && w.IsPublished, cancellationToken);

        if (definition == null)
            throw new KeyNotFoundException($"Published workflow definition '{workflowCode}' not found.");

        var startStep = definition.Steps.OrderBy(s => s.StepOrder).FirstOrDefault();
        if (startStep == null)
            throw new InvalidOperationException($"Workflow '{workflowCode}' has no configured steps.");

        var instance = new WorkflowInstance
        {
            WorkflowDefinitionId = definition.Id,
            WorkflowVersion = definition.Version, // Preserves version under which it started!
            EntityType = entityType,
            EntityId = entityId,
            CurrentStepId = startStep.Id,
            Status = "InProgress",
            InitiatedByUserId = userId,
            InitiatedAt = DateTimeOffset.UtcNow,
            CreatedBy = userId.ToString()
        };

        _context.WorkflowInstances.Add(instance);
        await _context.SaveChangesAsync(cancellationToken);

        _context.WorkflowHistories.Add(new WorkflowHistory
        {
            WorkflowInstanceId = instance.Id,
            StepId = startStep.Id,
            ActionTaken = "Initiated",
            ActionByUserId = userId,
            ActionAt = DateTimeOffset.UtcNow,
            PreviousStatus = "New",
            NewStatus = "InProgress",
            Comments = "Workflow initiated"
        });
        await _context.SaveChangesAsync(cancellationToken);

        return await BuildInstanceDtoAsync(instance.Id, cancellationToken);
    }

    public async Task<WorkflowInstanceDto> TransitionStepAsync(
        long instanceId,
        string actionName,
        string? comments,
        long userId,
        CancellationToken cancellationToken = default)
    {
        var instance = await _context.WorkflowInstances
            .Include(i => i.WorkflowDefinition)
                .ThenInclude(d => d!.Transitions)
            .Include(i => i.CurrentStep)
            .FirstOrDefaultAsync(i => i.Id == instanceId, cancellationToken);

        if (instance == null)
            throw new KeyNotFoundException($"Workflow instance #{instanceId} not found.");

        if (instance.Status != "InProgress")
            throw new InvalidOperationException($"Workflow instance is already finalized ({instance.Status}).");

        var transition = instance.WorkflowDefinition!.Transitions
            .FirstOrDefault(t => t.FromStepId == instance.CurrentStepId &&
                                 string.Equals(t.ActionName, actionName, StringComparison.OrdinalIgnoreCase));

        if (transition == null)
            throw new InvalidOperationException($"Action '{actionName}' is not permitted from current step '{instance.CurrentStep?.StepName}'.");

        var nextStep = await _context.WorkflowSteps.FirstOrDefaultAsync(s => s.Id == transition.ToStepId, cancellationToken);
        if (nextStep == null)
            throw new InvalidOperationException("Target workflow step does not exist.");

        string previousStatus = instance.Status;
        string newStatus = nextStep.StepType == "End" ? (actionName.Equals("Reject", StringComparison.OrdinalIgnoreCase) ? "Rejected" : "Approved") : "InProgress";

        instance.CurrentStepId = nextStep.Id;
        instance.Status = newStatus;
        if (newStatus != "InProgress")
        {
            instance.CompletedAt = DateTimeOffset.UtcNow;
        }

        _context.WorkflowHistories.Add(new WorkflowHistory
        {
            WorkflowInstanceId = instance.Id,
            StepId = nextStep.Id,
            ActionTaken = actionName,
            ActionByUserId = userId,
            ActionAt = DateTimeOffset.UtcNow,
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
            Comments = comments
        });

        await _context.SaveChangesAsync(cancellationToken);

        return await BuildInstanceDtoAsync(instance.Id, cancellationToken);
    }

    public async Task<WorkflowInstanceDto?> GetWorkflowInstanceAsync(long instanceId, CancellationToken cancellationToken = default)
    {
        var instance = await _context.WorkflowInstances.FirstOrDefaultAsync(i => i.Id == instanceId, cancellationToken);
        if (instance == null) return null;

        return await BuildInstanceDtoAsync(instanceId, cancellationToken);
    }

    private async Task<WorkflowInstanceDto> BuildInstanceDtoAsync(long instanceId, CancellationToken cancellationToken)
    {
        var instance = await _context.WorkflowInstances
            .Include(i => i.WorkflowDefinition)
                .ThenInclude(d => d!.Transitions)
            .Include(i => i.CurrentStep)
            .Include(i => i.Histories)
                .ThenInclude(h => h.Step)
            .FirstAsync(i => i.Id == instanceId, cancellationToken);

        var historyDtos = instance.Histories
            .OrderBy(h => h.ActionAt)
            .Select(h => new WorkflowHistoryDto(
                h.Id,
                h.Step?.StepName ?? "Step",
                h.ActionTaken,
                h.ActionByUserId,
                h.ActionAt,
                h.Comments,
                h.PreviousStatus,
                h.NewStatus
            ))
            .ToList();

        var availableActions = instance.WorkflowDefinition!.Transitions
            .Where(t => t.FromStepId == instance.CurrentStepId)
            .Select(t => t.ActionName)
            .Distinct()
            .ToList();

        return new WorkflowInstanceDto(
            instance.Id,
            instance.WorkflowDefinitionId,
            instance.WorkflowDefinition!.Title,
            instance.EntityType,
            instance.EntityId,
            instance.CurrentStepId,
            instance.CurrentStep?.StepName ?? "Current Step",
            instance.Status,
            instance.InitiatedByUserId,
            instance.InitiatedAt,
            historyDtos,
            availableActions
        );
    }
}
