using HotelOS.Domain.Inventory.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelOS.Infrastructure.Persistence.Configurations.Inventory;

public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("inventory_items", "inventory");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.ItemCode).HasMaxLength(32).IsRequired();
        builder.Property(i => i.ItemName).HasMaxLength(150).IsRequired();
        builder.Property(i => i.Category).HasMaxLength(64).IsRequired();
        builder.Property(i => i.UnitOfMeasure).HasMaxLength(32).HasDefaultValue("PCS");
        builder.Property(i => i.UnitCost).HasPrecision(18, 2);
        builder.Property(i => i.CurrentStock).HasPrecision(18, 2);
        builder.Property(i => i.ReorderLevel).HasPrecision(18, 2);
        builder.Property(i => i.MinimumStock).HasPrecision(18, 2);
        builder.Property(i => i.MaximumStock).HasPrecision(18, 2);

        builder.HasIndex(i => new { i.PropertyId, i.ItemCode }).IsUnique();
        builder.HasIndex(i => i.Category);
    }
}

public class StockTransactionConfiguration : IEntityTypeConfiguration<StockTransaction>
{
    public void Configure(EntityTypeBuilder<StockTransaction> builder)
    {
        builder.ToTable("stock_transactions", "inventory");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.TransactionNumber).HasMaxLength(64).IsRequired();
        builder.Property(t => t.TransactionType).HasMaxLength(32).IsRequired();
        builder.Property(t => t.Quantity).HasPrecision(18, 2);
        builder.Property(t => t.UnitCost).HasPrecision(18, 2);
        builder.Property(t => t.TotalValue).HasPrecision(18, 2);
        builder.Property(t => t.DepartmentCode).HasMaxLength(32);
        builder.Property(t => t.ReferenceNumber).HasMaxLength(64);

        builder.HasIndex(t => new { t.PropertyId, t.TransactionNumber }).IsUnique();
        builder.HasIndex(t => t.InventoryItemId);
        builder.HasIndex(t => t.TransactionType);

        builder.HasOne(t => t.InventoryItem)
            .WithMany()
            .HasForeignKey(t => t.InventoryItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
