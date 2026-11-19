using HotelOS.Domain.Common;

namespace HotelOS.Domain.Workflow.Entities;

public class WorkflowDefinition : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public int Version { get; set; } = 1;
    public bool IsPublished { get; set; } = true;
    public long? PropertyId { get; set; }
    public string Description { get; set; } = string.Empty;

    public ICollection<WorkflowStep> Steps { get; set; } = new List<WorkflowStep>();
    public ICollection<WorkflowTransition> Transitions { get; set; } = new List<WorkflowTransition>();
}
