using HotelOS.Domain.Housekeeping.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelOS.Infrastructure.Persistence.Configurations.Housekeeping;

public class HousekeepingTaskConfiguration : IEntityTypeConfiguration<HousekeepingTask>
{
    public void Configure(EntityTypeBuilder<HousekeepingTask> builder)
    {
        builder.ToTable("housekeeping_tasks", "housekeeping");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.TaskNumber).HasMaxLength(64).IsRequired();
        builder.Property(t => t.TaskType).HasMaxLength(64).IsRequired();
        builder.Property(t => t.Priority).HasMaxLength(32).IsRequired().HasDefaultValue("MEDIUM");
        builder.Property(t => t.Status).HasMaxLength(32).IsRequired().HasDefaultValue("PENDING");
        builder.Property(t => t.Remarks).HasMaxLength(500);
        builder.Property(t => t.ChecklistJson).HasColumnType("jsonb");

        builder.HasIndex(t => new { t.PropertyId, t.TaskNumber }).IsUnique();
        builder.HasIndex(t => t.RoomId);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.AssignedToUserId);

        builder.HasOne(t => t.Room)
            .WithMany()
            .HasForeignKey(t => t.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.AssignedToUser)
            .WithMany()
            .HasForeignKey(t => t.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.InspectedByUser)
            .WithMany()
            .HasForeignKey(t => t.InspectedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
