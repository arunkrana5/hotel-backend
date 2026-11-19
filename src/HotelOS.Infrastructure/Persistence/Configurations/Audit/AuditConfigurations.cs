using HotelOS.Domain.Audit.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelOS.Infrastructure.Persistence.Configurations.Audit;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs", "audit");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).UseIdentityByDefaultColumn();

        builder.Property(a => a.EntityName).HasMaxLength(100).IsRequired();
        builder.Property(a => a.EntityId).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Action).HasMaxLength(50).IsRequired();
        builder.Property(a => a.ChangesJson).HasColumnType("jsonb");
        builder.Property(a => a.PerformedBy).HasMaxLength(150).IsRequired();

        builder.HasIndex(a => new { a.EntityName, a.EntityId });
        builder.HasIndex(a => a.PerformedAt);
    }
}
