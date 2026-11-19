using HotelOS.Domain.Workflow.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelOS.Infrastructure.Persistence.Configurations.Workflow;

public class WorkflowDefinitionConfiguration : IEntityTypeConfiguration<WorkflowDefinition>
{
    public void Configure(EntityTypeBuilder<WorkflowDefinition> builder)
    {
        builder.ToTable("workflow_definitions", "workflow");

        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id).UseIdentityByDefaultColumn();

        builder.Property(w => w.Code).HasMaxLength(100).IsRequired();
        builder.HasIndex(w => new { w.Code, w.Version }).IsUnique();

        builder.Property(w => w.Title).HasMaxLength(200).IsRequired();
        builder.Property(w => w.Module).HasMaxLength(100).IsRequired();
    }
}

public class WorkflowStepConfiguration : IEntityTypeConfiguration<WorkflowStep>
{
    public void Configure(EntityTypeBuilder<WorkflowStep> builder)
    {
        builder.ToTable("workflow_steps", "workflow");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).UseIdentityByDefaultColumn();

        builder.Property(s => s.StepName).HasMaxLength(150).IsRequired();
        builder.Property(s => s.StepType).HasMaxLength(50).IsRequired();
        builder.Property(s => s.AssignedRole).HasMaxLength(100);

        builder.HasOne(s => s.WorkflowDefinition)
            .WithMany(w => w.Steps)
            .HasForeignKey(s => s.WorkflowDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class WorkflowTransitionConfiguration : IEntityTypeConfiguration<WorkflowTransition>
{
    public void Configure(EntityTypeBuilder<WorkflowTransition> builder)
    {
        builder.ToTable("workflow_transitions", "workflow");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).UseIdentityByDefaultColumn();

        builder.Property(t => t.ActionName).HasMaxLength(100).IsRequired();
        builder.Property(t => t.ConditionJson).HasColumnType("jsonb");

        builder.HasOne(t => t.WorkflowDefinition)
            .WithMany(w => w.Transitions)
            .HasForeignKey(t => t.WorkflowDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.FromStep)
            .WithMany()
            .HasForeignKey(t => t.FromStepId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.ToStep)
            .WithMany()
            .HasForeignKey(t => t.ToStepId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class WorkflowInstanceConfiguration : IEntityTypeConfiguration<WorkflowInstance>
{
    public void Configure(EntityTypeBuilder<WorkflowInstance> builder)
    {
        builder.ToTable("workflow_instances", "workflow");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).UseIdentityByDefaultColumn();

        builder.Property(i => i.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(i => i.Status).HasMaxLength(50).IsRequired();

        builder.HasOne(i => i.WorkflowDefinition)
            .WithMany()
            .HasForeignKey(i => i.WorkflowDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.CurrentStep)
            .WithMany()
            .HasForeignKey(i => i.CurrentStepId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => new { i.EntityType, i.EntityId });
    }
}

public class WorkflowHistoryConfiguration : IEntityTypeConfiguration<WorkflowHistory>
{
    public void Configure(EntityTypeBuilder<WorkflowHistory> builder)
    {
        builder.ToTable("workflow_histories", "workflow");

        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).UseIdentityByDefaultColumn();

        builder.Property(h => h.ActionTaken).HasMaxLength(100).IsRequired();
        builder.Property(h => h.PreviousStatus).HasMaxLength(50).IsRequired();
        builder.Property(h => h.NewStatus).HasMaxLength(50).IsRequired();
        builder.Property(h => h.Comments).HasMaxLength(1000);

        builder.HasOne(h => h.WorkflowInstance)
            .WithMany(i => i.Histories)
            .HasForeignKey(h => h.WorkflowInstanceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(h => h.Step)
            .WithMany()
            .HasForeignKey(h => h.StepId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
