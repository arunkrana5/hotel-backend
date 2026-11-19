using HotelOS.Domain.Common;

namespace HotelOS.Domain.Workflow.Entities;

public class WorkflowStep : BaseEntity
{
    public long WorkflowDefinitionId { get; set; }
    public int StepOrder { get; set; }
    public string StepName { get; set; } = string.Empty;
    public string StepType { get; set; } = "Approval"; // Start, Approval, Review, Notification, End
    public string? AssignedRole { get; set; }
    public long? AssignedDepartmentId { get; set; }
    public int RequiredApprovalCount { get; set; } = 1;
    public int SlaMinutes { get; set; } = 1440; // Default 24 hours
    public string? TimeoutAction { get; set; } // Escalate, AutoApprove, AutoReject

    public WorkflowDefinition? WorkflowDefinition { get; set; }
}
