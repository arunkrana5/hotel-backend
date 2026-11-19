using HotelOS.Domain.Security.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelOS.Infrastructure.Persistence.Configurations.Security;

public class VisitorLogConfiguration : IEntityTypeConfiguration<VisitorLog>
{
    public void Configure(EntityTypeBuilder<VisitorLog> builder)
    {
        builder.ToTable("visitor_logs", "security");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.PassNumber).HasMaxLength(64).IsRequired();
        builder.Property(v => v.VisitorName).HasMaxLength(150).IsRequired();
        builder.Property(v => v.IdNumber).HasMaxLength(64).IsRequired();
        builder.Property(v => v.Phone).HasMaxLength(32).IsRequired();
        builder.Property(v => v.Purpose).HasMaxLength(250).IsRequired();
        builder.Property(v => v.Status).HasMaxLength(32).HasDefaultValue("IN_PREMISES");

        builder.HasIndex(v => new { v.PropertyId, v.PassNumber }).IsUnique();
        builder.HasIndex(v => v.Status);
    }
}

public class IncidentReportConfiguration : IEntityTypeConfiguration<IncidentReport>
{
    public void Configure(EntityTypeBuilder<IncidentReport> builder)
    {
        builder.ToTable("incident_reports", "security");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.IncidentNumber).HasMaxLength(64).IsRequired();
        builder.Property(i => i.Location).HasMaxLength(150).IsRequired();
        builder.Property(i => i.Severity).HasMaxLength(32).HasDefaultValue("MEDIUM");
        builder.Property(i => i.Category).HasMaxLength(64).IsRequired();
        builder.Property(i => i.Description).HasMaxLength(2000).IsRequired();
        builder.Property(i => i.Status).HasMaxLength(32).HasDefaultValue("REPORTED");

        builder.HasIndex(i => new { i.PropertyId, i.IncidentNumber }).IsUnique();
        builder.HasIndex(i => i.Severity);
        builder.HasIndex(i => i.Status);

        builder.HasOne(i => i.ReportedByUser)
            .WithMany()
            .HasForeignKey(i => i.ReportedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
