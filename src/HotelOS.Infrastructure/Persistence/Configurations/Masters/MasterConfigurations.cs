using HotelOS.Domain.Masters.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelOS.Infrastructure.Persistence.Configurations.Masters;

public class MasterCategoryConfiguration : IEntityTypeConfiguration<MasterCategory>
{
    public void Configure(EntityTypeBuilder<MasterCategory> builder)
    {
        builder.ToTable("master_categories", "masters");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).UseIdentityByDefaultColumn();

        builder.Property(c => c.Code).HasMaxLength(100).IsRequired();
        builder.HasIndex(c => c.Code).IsUnique();

        builder.Property(c => c.Name).HasMaxLength(150).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(500);
    }
}

public class MasterDefinitionConfiguration : IEntityTypeConfiguration<MasterDefinition>
{
    public void Configure(EntityTypeBuilder<MasterDefinition> builder)
    {
        builder.ToTable("master_definitions", "masters");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).UseIdentityByDefaultColumn();

        builder.Property(d => d.Code).HasMaxLength(100).IsRequired();
        builder.HasIndex(d => d.Code).IsUnique();

        builder.Property(d => d.Name).HasMaxLength(150).IsRequired();
        builder.Property(d => d.Description).HasMaxLength(500);

        builder.HasOne(d => d.Category)
            .WithMany(c => c.Definitions)
            .HasForeignKey(d => d.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.ParentDefinition)
            .WithMany(p => p.ChildDefinitions)
            .HasForeignKey(d => d.ParentDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class MasterValueConfiguration : IEntityTypeConfiguration<MasterValue>
{
    public void Configure(EntityTypeBuilder<MasterValue> builder)
    {
        builder.ToTable("master_values", "masters");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).UseIdentityByDefaultColumn();

        builder.Property(v => v.Code).HasMaxLength(100).IsRequired();
        builder.Property(v => v.Name).HasMaxLength(200).IsRequired();
        builder.Property(v => v.Description).HasMaxLength(500);
        builder.Property(v => v.NumericValue).HasPrecision(18, 4);
        builder.Property(v => v.ExtraDataJson).HasColumnType("jsonb");

        builder.HasOne(v => v.MasterDefinition)
            .WithMany(d => d.Values)
            .HasForeignKey(v => v.MasterDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.ParentValue)
            .WithMany(p => p.ChildValues)
            .HasForeignKey(v => v.ParentValueId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(v => new { v.MasterDefinitionId, v.PropertyId, v.Code });
        builder.HasIndex(v => new { v.MasterDefinitionId, v.IsActive });
    }
}
