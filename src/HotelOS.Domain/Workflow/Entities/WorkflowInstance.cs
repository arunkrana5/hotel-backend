using HotelOS.Domain.Common;

namespace HotelOS.Domain.Workflow.Entities;

public class WorkflowInstance : BaseEntity
{
    public long WorkflowDefinitionId { get; set; }
    public int WorkflowVersion { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public long EntityId { get; set; }
    public long CurrentStepId { get; set; }
    public string Status { get; set; } = "InProgress"; // InProgress, Approved, Rejected, Cancelled
    public long InitiatedByUserId { get; set; }
    public DateTimeOffset InitiatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; set; }

    public WorkflowDefinition? WorkflowDefinition { get; set; }
    public WorkflowStep? CurrentStep { get; set; }
    public ICollection<WorkflowHistory> Histories { get; set; } = new List<WorkflowHistory>();
}
