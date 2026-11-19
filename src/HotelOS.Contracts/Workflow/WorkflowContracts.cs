namespace HotelOS.Contracts.Workflow;

public record WorkflowStepDto(
    long Id,
    int StepOrder,
    string StepName,
    string StepType,
    string? AssignedRole,
    int SlaMinutes
);

public record WorkflowDefinitionDto(
    long Id,
    string Code,
    string Title,
    string Module,
    int Version,
    string Description,
    List<WorkflowStepDto> Steps
);

public record WorkflowTransitionRequest(
    long InstanceId,
    string ActionName,
    string? Comments
);

public record WorkflowInstanceDto(
    long Id,
    long WorkflowDefinitionId,
    string WorkflowTitle,
    string EntityType,
    long EntityId,
    long CurrentStepId,
    string CurrentStepName,
    string Status,
    long InitiatedByUserId,
    DateTimeOffset InitiatedAt,
    List<WorkflowHistoryDto> History,
    List<string> AvailableActions
);

public record WorkflowHistoryDto(
    long Id,
    string StepName,
    string ActionTaken,
    long ActionByUserId,
    DateTimeOffset ActionAt,
    string? Comments,
    string PreviousStatus,
    string NewStatus
);
