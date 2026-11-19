using HotelOS.Domain.Maintenance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelOS.Infrastructure.Persistence.Configurations.Maintenance;

public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.ToTable("assets", "maintenance");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.AssetCode).HasMaxLength(32).IsRequired();
        builder.Property(a => a.AssetName).HasMaxLength(150).IsRequired();
        builder.Property(a => a.Category).HasMaxLength(64).IsRequired();
        builder.Property(a => a.Location).HasMaxLength(150).IsRequired();
        builder.Property(a => a.PurchaseCost).HasPrecision(18, 2);
        builder.Property(a => a.Status).HasMaxLength(32).HasDefaultValue("OPERATIONAL");

        builder.HasIndex(a => new { a.PropertyId, a.AssetCode }).IsUnique();
        builder.HasIndex(a => a.Category);
        builder.HasIndex(a => a.Status);
    }
}

public class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.ToTable("work_orders", "maintenance");
        builder.HasKey(w => w.Id);
        builder.Property(w => w.WorkOrderNumber).HasMaxLength(64).IsRequired();
        builder.Property(w => w.DepartmentCode).HasMaxLength(32).IsRequired();
        builder.Property(w => w.IssueDescription).HasMaxLength(1000).IsRequired();
        builder.Property(w => w.Priority).HasMaxLength(32).HasDefaultValue("MEDIUM");
        builder.Property(w => w.Status).HasMaxLength(32).HasDefaultValue("OPEN");
        builder.Property(w => w.TotalCost).HasPrecision(18, 2);

        builder.HasIndex(w => new { w.PropertyId, w.WorkOrderNumber }).IsUnique();
        builder.HasIndex(w => w.Status);

        builder.HasOne(w => w.Asset)
            .WithMany()
            .HasForeignKey(w => w.AssetId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(w => w.Room)
            .WithMany()
            .HasForeignKey(w => w.RoomId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(w => w.AssignedToUser)
            .WithMany()
            .HasForeignKey(w => w.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
