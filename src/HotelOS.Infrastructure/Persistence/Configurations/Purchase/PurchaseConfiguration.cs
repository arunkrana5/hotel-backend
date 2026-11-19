using HotelOS.Domain.Purchase.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelOS.Infrastructure.Persistence.Configurations.Purchase;

public class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> builder)
    {
        builder.ToTable("vendors", "purchase");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.VendorCode).HasMaxLength(32).IsRequired();
        builder.Property(v => v.VendorName).HasMaxLength(150).IsRequired();
        builder.Property(v => v.ContactPerson).HasMaxLength(100);
        builder.Property(v => v.Email).HasMaxLength(150);
        builder.Property(v => v.Phone).HasMaxLength(32);
        builder.Property(v => v.Status).HasMaxLength(32).HasDefaultValue("ACTIVE");

        builder.HasIndex(v => new { v.PropertyId, v.VendorCode }).IsUnique();
        builder.HasIndex(v => v.Status);
    }
}

public class PurchaseRequisitionConfiguration : IEntityTypeConfiguration<PurchaseRequisition>
{
    public void Configure(EntityTypeBuilder<PurchaseRequisition> builder)
    {
        builder.ToTable("purchase_requisitions", "purchase");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.RequisitionNumber).HasMaxLength(64).IsRequired();
        builder.Property(r => r.DepartmentCode).HasMaxLength(32).IsRequired();
        builder.Property(r => r.Priority).HasMaxLength(32).HasDefaultValue("MEDIUM");
        builder.Property(r => r.Status).HasMaxLength(32).HasDefaultValue("DRAFT");
        builder.Property(r => r.TotalEstimatedAmount).HasPrecision(18, 2);
        builder.Property(r => r.Justification).HasMaxLength(500);

        builder.HasIndex(r => new { r.PropertyId, r.RequisitionNumber }).IsUnique();
        builder.HasIndex(r => r.Status);

        builder.HasMany(r => r.Items)
            .WithOne(i => i.PurchaseRequisition)
            .HasForeignKey(i => i.PurchaseRequisitionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PurchaseRequisitionItemConfiguration : IEntityTypeConfiguration<PurchaseRequisitionItem>
{
    public void Configure(EntityTypeBuilder<PurchaseRequisitionItem> builder)
    {
        builder.ToTable("purchase_requisition_items", "purchase");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.ItemName).HasMaxLength(150).IsRequired();
        builder.Property(i => i.Quantity).HasPrecision(18, 2);
        builder.Property(i => i.EstimatedUnitPrice).HasPrecision(18, 2);
        builder.Property(i => i.TotalEstimatedPrice).HasPrecision(18, 2);
    }
}

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("purchase_orders", "purchase");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.OrderNumber).HasMaxLength(64).IsRequired();
        builder.Property(o => o.Status).HasMaxLength(32).HasDefaultValue("ISSUED");
        builder.Property(o => o.SubTotal).HasPrecision(18, 2);
        builder.Property(o => o.TaxAmount).HasPrecision(18, 2);
        builder.Property(o => o.TotalAmount).HasPrecision(18, 2);

        builder.HasIndex(o => new { o.PropertyId, o.OrderNumber }).IsUnique();
        builder.HasIndex(o => o.VendorId);
        builder.HasIndex(o => o.Status);

        builder.HasOne(o => o.Vendor)
            .WithMany()
            .HasForeignKey(o => o.VendorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.Items)
            .WithOne(i => i.PurchaseOrder)
            .HasForeignKey(i => i.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PurchaseOrderItemConfiguration : IEntityTypeConfiguration<PurchaseOrderItem>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderItem> builder)
    {
        builder.ToTable("purchase_order_items", "purchase");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.ItemName).HasMaxLength(150).IsRequired();
        builder.Property(i => i.QuantityOrdered).HasPrecision(18, 2);
        builder.Property(i => i.QuantityReceived).HasPrecision(18, 2);
        builder.Property(i => i.UnitPrice).HasPrecision(18, 2);
        builder.Property(i => i.TotalPrice).HasPrecision(18, 2);
    }
}
