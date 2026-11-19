using HotelOS.Domain.Configuration.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelOS.Infrastructure.Persistence.Configurations.Configuration;

public class NavigationMenuConfiguration : IEntityTypeConfiguration<NavigationMenu>
{
    public void Configure(EntityTypeBuilder<NavigationMenu> builder)
    {
        builder.ToTable("navigation_menus", "configuration");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).UseIdentityByDefaultColumn();

        builder.Property(m => m.Code).HasMaxLength(100).IsRequired();
        builder.HasIndex(m => m.Code).IsUnique();

        builder.Property(m => m.Title).HasMaxLength(150).IsRequired();
        builder.Property(m => m.Path).HasMaxLength(250).IsRequired();
        builder.Property(m => m.Icon).HasMaxLength(50);
        builder.Property(m => m.Module).HasMaxLength(100).IsRequired();

        builder.HasOne(m => m.Parent)
            .WithMany(p => p.Children)
            .HasForeignKey(m => m.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class DynamicFormDefinitionConfiguration : IEntityTypeConfiguration<DynamicFormDefinition>
{
    public void Configure(EntityTypeBuilder<DynamicFormDefinition> builder)
    {
        builder.ToTable("dynamic_form_definitions", "configuration");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).UseIdentityByDefaultColumn();

        builder.Property(f => f.Code).HasMaxLength(100).IsRequired();
        builder.HasIndex(f => new { f.Code, f.Version }).IsUnique();

        builder.Property(f => f.Title).HasMaxLength(200).IsRequired();
        builder.Property(f => f.Module).HasMaxLength(100).IsRequired();
        builder.Property(f => f.EntityType).HasMaxLength(100).IsRequired();
    }
}

public class DynamicFormFieldConfiguration : IEntityTypeConfiguration<DynamicFormField>
{
    public void Configure(EntityTypeBuilder<DynamicFormField> builder)
    {
        builder.ToTable("dynamic_form_fields", "configuration");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).UseIdentityByDefaultColumn();

        builder.Property(f => f.FieldKey).HasMaxLength(100).IsRequired();
        builder.Property(f => f.FieldLabel).HasMaxLength(150).IsRequired();
        builder.Property(f => f.FieldType).HasMaxLength(50).IsRequired();
        builder.Property(f => f.MasterDefinitionCode).HasMaxLength(100);
        builder.Property(f => f.ValidationRulesJson).HasColumnType("jsonb");
        builder.Property(f => f.VisibilityRuleJson).HasColumnType("jsonb");

        builder.HasOne(f => f.FormDefinition)
            .WithMany(d => d.Fields)
            .HasForeignKey(f => f.FormDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class DocumentNumberConfigurationEntityTypeConfiguration : IEntityTypeConfiguration<DocumentNumberConfiguration>
{
    public void Configure(EntityTypeBuilder<DocumentNumberConfiguration> builder)
    {
        builder.ToTable("document_number_configurations", "configuration");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).UseIdentityByDefaultColumn();

        builder.Property(c => c.DocumentType).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Module).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Prefix).HasMaxLength(20).IsRequired();
        builder.Property(c => c.Suffix).HasMaxLength(20);
        builder.Property(c => c.DateFormat).HasMaxLength(20).IsRequired();
        builder.Property(c => c.ResetFrequency).HasMaxLength(20).IsRequired();
        builder.Property(c => c.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(c => c.UpdatedBy).HasMaxLength(100);

        builder.HasIndex(c => new { c.PropertyId, c.DocumentType }).IsUnique();
    }
}

public class DocumentNumberAuditEntityTypeConfiguration : IEntityTypeConfiguration<DocumentNumberAudit>
{
    public void Configure(EntityTypeBuilder<DocumentNumberAudit> builder)
    {
        builder.ToTable("document_number_audits", "configuration");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).UseIdentityByDefaultColumn();

        builder.Property(a => a.DocumentType).HasMaxLength(100).IsRequired();
        builder.Property(a => a.DocumentNumber).HasMaxLength(100).IsRequired();
        builder.Property(a => a.GeneratedBy).HasMaxLength(100).IsRequired();
        builder.Property(a => a.EntityType).HasMaxLength(100);
        builder.Property(a => a.Status).HasMaxLength(50).IsRequired();

        builder.HasIndex(a => new { a.PropertyId, a.DocumentType });
        builder.HasIndex(a => a.DocumentNumber);
    }
}
