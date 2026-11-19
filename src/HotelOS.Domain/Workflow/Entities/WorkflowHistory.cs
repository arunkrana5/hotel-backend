namespace HotelOS.Domain.Workflow.Entities;

public class WorkflowHistory
{
    public long Id { get; set; }
    public long WorkflowInstanceId { get; set; }
    public long StepId { get; set; }
    public string ActionTaken { get; set; } = string.Empty;
    public long ActionByUserId { get; set; }
    public DateTimeOffset ActionAt { get; set; } = DateTimeOffset.UtcNow;
    public string? Comments { get; set; }
    public string PreviousStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;

    public WorkflowInstance? WorkflowInstance { get; set; }
    public WorkflowStep? Step { get; set; }
}
