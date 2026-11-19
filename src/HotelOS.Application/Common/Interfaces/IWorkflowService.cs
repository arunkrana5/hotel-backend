using HotelOS.Contracts.Workflow;

namespace HotelOS.Application.Common.Interfaces;

public interface IWorkflowService
{
    Task<List<WorkflowDefinitionDto>> GetAllDefinitionsAsync(CancellationToken cancellationToken = default);

    Task<WorkflowDefinitionDto?> GetDefinitionAsync(string code, CancellationToken cancellationToken = default);

    Task<WorkflowInstanceDto> StartWorkflowAsync(
        string workflowCode,
        string entityType,
        long entityId,
        long userId,
        CancellationToken cancellationToken = default);

    Task<WorkflowInstanceDto> TransitionStepAsync(
        long instanceId,
        string actionName,
        string? comments,
        long userId,
        CancellationToken cancellationToken = default);

    Task<WorkflowInstanceDto?> GetWorkflowInstanceAsync(
        long instanceId,
        CancellationToken cancellationToken = default);
}
