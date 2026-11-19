using HotelOS.Domain.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelOS.Infrastructure.Persistence.Configurations.Identity;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", "identity");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).UseIdentityByDefaultColumn();

        builder.Property(u => u.PublicId).IsRequired();
        builder.HasIndex(u => u.PublicId).IsUnique();

        builder.Property(u => u.Username).HasMaxLength(100).IsRequired();
        builder.Property(u => u.NormalizedUsername).HasMaxLength(100).IsRequired();
        builder.HasIndex(u => u.NormalizedUsername).IsUnique();

        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.Property(u => u.NormalizedEmail).HasMaxLength(256).IsRequired();
        builder.HasIndex(u => u.NormalizedEmail).IsUnique();

        builder.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.LastName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.PhoneNumber).HasMaxLength(30);
        builder.Property(u => u.EmployeeId).HasMaxLength(50);

        builder.HasOne(u => u.Credential)
            .WithOne(c => c.User)
            .HasForeignKey<UserCredential>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class UserCredentialConfiguration : IEntityTypeConfiguration<UserCredential>
{
    public void Configure(EntityTypeBuilder<UserCredential> builder)
    {
        builder.ToTable("user_credentials", "identity");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).UseIdentityByDefaultColumn();

        builder.Property(c => c.PasswordHash).IsRequired();
        builder.Property(c => c.SecurityStamp).HasMaxLength(100).IsRequired();
    }
}

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles", "identity");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).UseIdentityByDefaultColumn();

        builder.Property(r => r.Name).HasMaxLength(100).IsRequired();
        builder.Property(r => r.NormalizedName).HasMaxLength(100).IsRequired();
        builder.HasIndex(r => r.NormalizedName).IsUnique();

        builder.Property(r => r.Description).HasMaxLength(500);
    }
}

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_roles", "identity");

        builder.HasKey(ur => new { ur.UserId, ur.RoleId });

        builder.HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissions", "identity");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).UseIdentityByDefaultColumn();

        builder.Property(p => p.Code).HasMaxLength(150).IsRequired();
        builder.HasIndex(p => p.Code).IsUnique();

        builder.Property(p => p.Module).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Action).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(500);
    }
}

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("role_permissions", "identity");

        builder.HasKey(rp => new { rp.RoleId, rp.PermissionId });

        builder.HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("user_sessions", "identity");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.IpAddress).HasMaxLength(64).IsRequired();
        builder.Property(s => s.UserAgent).HasMaxLength(500).IsRequired();
        builder.Property(s => s.DeviceInfo).HasMaxLength(250);

        builder.HasOne(s => s.User)
            .WithMany(u => u.Sessions)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens", "identity");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).UseIdentityByDefaultColumn();

        builder.Property(t => t.TokenHash).HasMaxLength(256).IsRequired();
        builder.HasIndex(t => t.TokenHash).IsUnique();

        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Session)
            .WithMany(s => s.RefreshTokens)
            .HasForeignKey(t => t.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("password_reset_tokens", "identity");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).UseIdentityByDefaultColumn();

        builder.Property(t => t.TokenHash).HasMaxLength(256).IsRequired();
        builder.HasIndex(t => t.TokenHash);

        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class EmailVerificationTokenConfiguration : IEntityTypeConfiguration<EmailVerificationToken>
{
    public void Configure(EntityTypeBuilder<EmailVerificationToken> builder)
    {
        builder.ToTable("email_verification_tokens", "identity");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).UseIdentityByDefaultColumn();

        builder.Property(t => t.TokenHash).HasMaxLength(256).IsRequired();
        builder.HasIndex(t => t.TokenHash);

        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class LoginAttemptConfiguration : IEntityTypeConfiguration<LoginAttempt>
{
    public void Configure(EntityTypeBuilder<LoginAttempt> builder)
    {
        builder.ToTable("login_attempts", "identity");

        builder.HasKey(la => la.Id);
        builder.Property(la => la.Id).UseIdentityByDefaultColumn();

        builder.Property(la => la.UsernameOrEmail).HasMaxLength(256).IsRequired();
        builder.Property(la => la.IpAddress).HasMaxLength(64).IsRequired();
        builder.Property(la => la.UserAgent).HasMaxLength(500);
        builder.Property(la => la.FailureReason).HasMaxLength(250);

        builder.HasIndex(la => la.AttemptedAt);
    }
}

public class LoginHistoryConfiguration : IEntityTypeConfiguration<LoginHistory>
{
    public void Configure(EntityTypeBuilder<LoginHistory> builder)
    {
        builder.ToTable("login_history", "identity");

        builder.HasKey(lh => lh.Id);
        builder.Property(lh => lh.Id).UseIdentityByDefaultColumn();

        builder.Property(lh => lh.IpAddress).HasMaxLength(64).IsRequired();
        builder.Property(lh => lh.UserAgent).HasMaxLength(500);

        builder.HasOne(lh => lh.User)
            .WithMany(u => u.LoginHistories)
            .HasForeignKey(lh => lh.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(lh => lh.LoginAt);
    }
}

public class PasswordHistoryConfiguration : IEntityTypeConfiguration<PasswordHistory>
{
    public void Configure(EntityTypeBuilder<PasswordHistory> builder)
    {
        builder.ToTable("password_history", "identity");

        builder.HasKey(ph => ph.Id);
        builder.Property(ph => ph.Id).UseIdentityByDefaultColumn();

        builder.Property(ph => ph.PasswordHash).IsRequired();

        builder.HasOne(ph => ph.User)
            .WithMany(u => u.PasswordHistories)
            .HasForeignKey(ph => ph.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class SecurityEventConfiguration : IEntityTypeConfiguration<SecurityEvent>
{
    public void Configure(EntityTypeBuilder<SecurityEvent> builder)
    {
        builder.ToTable("security_events", "identity");

        builder.HasKey(se => se.Id);
        builder.Property(se => se.Id).UseIdentityByDefaultColumn();

        builder.Property(se => se.Description).HasMaxLength(500).IsRequired();
        builder.Property(se => se.IpAddress).HasMaxLength(64).IsRequired();
        builder.Property(se => se.UserAgent).HasMaxLength(500);

        builder.HasOne(se => se.User)
            .WithMany(u => u.SecurityEvents)
            .HasForeignKey(se => se.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(se => se.OccurredAt);
    }
}

public class UserPropertyAssignmentConfiguration : IEntityTypeConfiguration<UserPropertyAssignment>
{
    public void Configure(EntityTypeBuilder<UserPropertyAssignment> builder)
    {
        builder.ToTable("user_property_assignments", "identity");

        builder.HasKey(upa => upa.Id);
        builder.Property(upa => upa.Id).UseIdentityByDefaultColumn();

        builder.HasOne(upa => upa.User)
            .WithMany(u => u.PropertyAssignments)
            .HasForeignKey(upa => upa.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(upa => new { upa.UserId, upa.PropertyId }).IsUnique();
    }
}

public class AppNotificationConfiguration : IEntityTypeConfiguration<AppNotification>
{
    public void Configure(EntityTypeBuilder<AppNotification> builder)
    {
        builder.ToTable("notifications", "platform");

        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).UseIdentityByDefaultColumn();

        builder.Property(n => n.Title).HasMaxLength(200).IsRequired();
        builder.Property(n => n.Message).HasMaxLength(1000).IsRequired();
        builder.Property(n => n.Type).HasMaxLength(50).IsRequired();
        builder.Property(n => n.TargetUrl).HasMaxLength(250);

        builder.HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
