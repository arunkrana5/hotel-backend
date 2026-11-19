using HotelOS.Domain.Common;

namespace HotelOS.Domain.Workflow.Entities;

public class WorkflowTransition : BaseEntity
{
    public long WorkflowDefinitionId { get; set; }
    public long FromStepId { get; set; }
    public long ToStepId { get; set; }
    public string ActionName { get; set; } = string.Empty; // Submit, Approve, Reject, Resubmit, Cancel
    public string? ConditionJson { get; set; } // e.g. amount > 5000

    public WorkflowDefinition? WorkflowDefinition { get; set; }
    public WorkflowStep? FromStep { get; set; }
    public WorkflowStep? ToStep { get; set; }
}
