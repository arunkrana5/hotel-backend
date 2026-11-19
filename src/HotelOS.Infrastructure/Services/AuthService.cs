using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Common;
using HotelOS.Contracts.Identity;
using HotelOS.Domain.Identity.Entities;
using HotelOS.Domain.Identity.Enums;
using HotelOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelOS.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly HotelOSDbContext _context;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthService> _logger;

    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan ResetTokenDuration = TimeSpan.FromMinutes(15);

    public AuthService(
        HotelOSDbContext context,
        IPasswordService passwordService,
        ITokenService tokenService,
        IEmailService emailService,
        ILogger<AuthService> logger)
    {
        _context = context;
        _passwordService = passwordService;
        _tokenService = tokenService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<ApiResponse<AuthTokenResponse>> LoginAsync(
        LoginRequest request,
        string ipAddress,
        string userAgent,
        CancellationToken cancellationToken = default)
    {
        string normalized = request.UsernameOrEmail.Trim().ToUpperInvariant();

        var user = await _context.Users
            .Include(u => u.Credential)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r!.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .Include(u => u.PropertyAssignments)
            .FirstOrDefaultAsync(u => u.NormalizedUsername == normalized || u.NormalizedEmail == normalized, cancellationToken);

        if (user == null)
        {
            await RecordLoginAttemptAsync(request.UsernameOrEmail, ipAddress, userAgent, false, "User not found", cancellationToken);
            return ApiResponse<AuthTokenResponse>.Fail("Invalid credentials", 401);
        }

        // Account status checks
        if (user.Status == AccountStatus.Inactive)
        {
            await RecordLoginAttemptAsync(request.UsernameOrEmail, ipAddress, userAgent, false, "Account inactive", cancellationToken);
            return ApiResponse<AuthTokenResponse>.Fail("Your account is inactive. Please contact your IT administrator.", 403);
        }

        if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow)
        {
            var remaining = (user.LockoutEnd.Value - DateTimeOffset.UtcNow).Minutes + 1;
            await RecordLoginAttemptAsync(request.UsernameOrEmail, ipAddress, userAgent, false, "Account locked", cancellationToken);
            return ApiResponse<AuthTokenResponse>.Fail($"Account is temporarily locked due to failed attempts. Please try again in {remaining} minute(s).", 423);
        }

        if (user.Status == AccountStatus.PendingFirstLogin)
        {
            return ApiResponse<AuthTokenResponse>.Fail("First-time login setup required. Please use the invitation link sent to your email.", 403);
        }

        // Password verification
        if (user.Credential == null || !_passwordService.VerifyPassword(request.Password, user.Credential.PasswordHash))
        {
            user.AccessFailedCount++;
            if (user.AccessFailedCount >= MaxFailedAttempts)
            {
                user.LockoutEnd = DateTimeOffset.UtcNow.Add(LockoutDuration);
                user.Status = AccountStatus.Locked;

                _context.SecurityEvents.Add(new SecurityEvent
                {
                    UserId = user.Id,
                    EventType = SecurityEventType.AccountLocked,
                    Description = $"Account locked after {user.AccessFailedCount} consecutive failed attempts.",
                    IpAddress = ipAddress,
                    UserAgent = userAgent
                });
            }

            await RecordLoginAttemptAsync(request.UsernameOrEmail, ipAddress, userAgent, false, "Incorrect password", cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            int remainingAttempts = Math.Max(0, MaxFailedAttempts - user.AccessFailedCount);
            string failMsg = remainingAttempts > 0
                ? $"Invalid credentials. {remainingAttempts} attempt(s) remaining before temporary lockout."
                : $"Invalid credentials. Account locked for {LockoutDuration.TotalMinutes} minutes.";

            return ApiResponse<AuthTokenResponse>.Fail(failMsg, 401);
        }

        // Reset lockout upon successful password
        user.AccessFailedCount = 0;
        user.LockoutEnd = null;
        user.LastLoginAt = DateTimeOffset.UtcNow;
        if (user.Status == AccountStatus.Locked)
            user.Status = AccountStatus.Active;

        // Create new Session
        var session = new UserSession
        {
            UserId = user.Id,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            DeviceInfo = request.DeviceInfo,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(request.RememberMe ? 30 : 7)
        };
        _context.UserSessions.Add(session);

        // Generate Refresh Token
        var (rawRefreshToken, tokenHash, refreshExpiresAt) = _tokenService.GenerateRefreshToken();
        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            SessionId = session.Id,
            TokenHash = tokenHash,
            ExpiresAt = refreshExpiresAt
        };
        _context.RefreshTokens.Add(refreshTokenEntity);

        // Record history
        _context.LoginHistories.Add(new LoginHistory
        {
            UserId = user.Id,
            SessionId = session.Id,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            LoginAt = DateTimeOffset.UtcNow
        });

        _context.SecurityEvents.Add(new SecurityEvent
        {
            UserId = user.Id,
            EventType = SecurityEventType.LoginSuccess,
            Description = "User logged in successfully.",
            IpAddress = ipAddress,
            UserAgent = userAgent
        });

        await RecordLoginAttemptAsync(request.UsernameOrEmail, ipAddress, userAgent, true, null, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Extract roles & permissions
        var roles = user.UserRoles.Select(ur => ur.Role!.Name).Distinct().ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role!.RolePermissions)
            .Select(rp => rp.Permission!.Code)
            .Distinct()
            .ToList();

        string accessToken = _tokenService.GenerateAccessToken(user, roles, permissions);

        var userProfile = BuildUserProfileDto(user, roles, permissions);
        var response = new AuthTokenResponse(accessToken, rawRefreshToken, session.ExpiresAt, userProfile);

        return ApiResponse<AuthTokenResponse>.Ok(response, "Login successful");
    }

    public async Task<ApiResponse<AuthTokenResponse>> RefreshTokenAsync(
        RefreshTokenRequest request,
        string ipAddress,
        string userAgent,
        CancellationToken cancellationToken = default)
    {
        string incomingHash = _tokenService.HashToken(request.RefreshToken);

        var token = await _context.RefreshTokens
            .Include(t => t.Session)
            .Include(t => t.User)
                .ThenInclude(u => u!.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r!.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
            .Include(t => t.User)
                .ThenInclude(u => u!.PropertyAssignments)
            .FirstOrDefaultAsync(t => t.TokenHash == incomingHash, cancellationToken);

        if (token == null || token.IsRevoked || token.ExpiresAt < DateTimeOffset.UtcNow || token.Session == null || token.Session.IsRevoked)
        {
            return ApiResponse<AuthTokenResponse>.Fail("Invalid or expired refresh token", 401);
        }

        var user = token.User;
        if (user == null || user.Status != AccountStatus.Active)
        {
            return ApiResponse<AuthTokenResponse>.Fail("Account is inactive or not found", 401);
        }

        // Token Rotation: Invalidate used token
        token.IsRevoked = true;
        token.RevokedAt = DateTimeOffset.UtcNow;

        // Generate new refresh token
        var (newRawRefreshToken, newTokenHash, newExpiresAt) = _tokenService.GenerateRefreshToken();
        token.ReplacedByTokenHash = newTokenHash;

        var newRefreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            SessionId = token.SessionId,
            TokenHash = newTokenHash,
            ExpiresAt = newExpiresAt
        };
        _context.RefreshTokens.Add(newRefreshTokenEntity);

        _context.SecurityEvents.Add(new SecurityEvent
        {
            UserId = user.Id,
            EventType = SecurityEventType.TokenRefreshed,
            Description = "Token refreshed via refresh token rotation.",
            IpAddress = ipAddress,
            UserAgent = userAgent
        });

        await _context.SaveChangesAsync(cancellationToken);

        var roles = user.UserRoles.Select(ur => ur.Role!.Name).Distinct().ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role!.RolePermissions)
            .Select(rp => rp.Permission!.Code)
            .Distinct()
            .ToList();

        string accessToken = _tokenService.GenerateAccessToken(user, roles, permissions);
        var userProfile = BuildUserProfileDto(user, roles, permissions);

        return ApiResponse<AuthTokenResponse>.Ok(
            new AuthTokenResponse(accessToken, newRawRefreshToken, newExpiresAt, userProfile),
            "Token refreshed successfully");
    }

    public async Task<ApiResponse> LogoutAsync(string? refreshToken, long? userId, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            string tokenHash = _tokenService.HashToken(refreshToken);
            var token = await _context.RefreshTokens
                .Include(t => t.Session)
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

            if (token != null)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTimeOffset.UtcNow;

                if (token.Session != null)
                {
                    token.Session.IsRevoked = true;
                    token.Session.RevokedAt = DateTimeOffset.UtcNow;
                    token.Session.RevokedReason = "User logged out";
                }
            }
        }

        if (userId.HasValue)
        {
            _context.SecurityEvents.Add(new SecurityEvent
            {
                UserId = userId.Value,
                EventType = SecurityEventType.Logout,
                Description = "User logged out.",
                IpAddress = "N/A",
                UserAgent = "N/A"
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponse.OkMessage("Logged out successfully");
    }

    public async Task<ApiResponse> ForgotPasswordAsync(
        ForgotPasswordRequest request,
        string ipAddress,
        string userAgent,
        CancellationToken cancellationToken = default)
    {
        string normalized = request.Email.Trim().ToUpperInvariant();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalized, cancellationToken);

        // Security best practice: Do not reveal if email exists
        if (user != null && user.Status == AccountStatus.Active)
        {
            var (rawToken, tokenHash, expiresAt) = _tokenService.GenerateCryptoToken(ResetTokenDuration);

            _context.PasswordResetTokens.Add(new PasswordResetToken
            {
                UserId = user.Id,
                TokenHash = tokenHash,
                ExpiresAt = expiresAt
            });

            _context.SecurityEvents.Add(new SecurityEvent
            {
                UserId = user.Id,
                EventType = SecurityEventType.PasswordResetRequested,
                Description = "Password reset link requested.",
                IpAddress = ipAddress,
                UserAgent = userAgent
            });

            await _context.SaveChangesAsync(cancellationToken);

            await _emailService.SendPasswordResetEmailAsync(
                user.Email,
                rawToken,
                $"{user.FirstName} {user.LastName}",
                cancellationToken);
        }

        return ApiResponse.OkMessage("If the email is registered and active, a password reset link has been dispatched.");
    }

    public async Task<ApiResponse<ResetTokenVerificationResponse>> VerifyResetTokenAsync(
        VerifyResetTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        string normalized = request.Email.Trim().ToUpperInvariant();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalized, cancellationToken);

        if (user == null)
        {
            return ApiResponse<ResetTokenVerificationResponse>.Ok(
                new ResetTokenVerificationResponse(false, request.Email, "Invalid or expired token"));
        }

        string tokenHash = _tokenService.HashToken(request.Token);
        var tokenRecord = await _context.PasswordResetTokens
            .Where(t => t.UserId == user.Id && !t.IsUsed && t.ExpiresAt > DateTimeOffset.UtcNow)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (tokenRecord == null)
        {
            return ApiResponse<ResetTokenVerificationResponse>.Ok(
                new ResetTokenVerificationResponse(false, request.Email, "Token has expired, was already used, or is invalid."));
        }

        return ApiResponse<ResetTokenVerificationResponse>.Ok(
            new ResetTokenVerificationResponse(true, request.Email, "Token is valid."));
    }

    public async Task<ApiResponse> ResetPasswordAsync(
        ResetPasswordRequest request,
        string ipAddress,
        string userAgent,
        CancellationToken cancellationToken = default)
    {
        if (request.NewPassword != request.ConfirmNewPassword)
        {
            return ApiResponse.FailMessage("Passwords do not match.", 400);
        }

        if (request.NewPassword.Length < 8)
        {
            return ApiResponse.FailMessage("Password must be at least 8 characters long.", 400);
        }

        string normalized = request.Email.Trim().ToUpperInvariant();
        var user = await _context.Users
            .Include(u => u.Credential)
            .Include(u => u.PasswordHistories)
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalized, cancellationToken);

        if (user == null)
        {
            return ApiResponse.FailMessage("Invalid request.", 400);
        }

        string tokenHash = _tokenService.HashToken(request.Token);
        var tokenRecord = await _context.PasswordResetTokens
            .Where(t => t.UserId == user.Id && !t.IsUsed && t.ExpiresAt > DateTimeOffset.UtcNow)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (tokenRecord == null)
        {
            return ApiResponse.FailMessage("Password reset token is invalid or has expired.", 400);
        }

        // Prevent password reuse from recent history
        foreach (var history in user.PasswordHistories.OrderByDescending(h => h.CreatedAt).Take(5))
        {
            if (_passwordService.VerifyPassword(request.NewPassword, history.PasswordHash))
            {
                return ApiResponse.FailMessage("You cannot reuse any of your last 5 passwords.", 400);
            }
        }

        // Invalidate token
        tokenRecord.IsUsed = true;
        tokenRecord.UsedAt = DateTimeOffset.UtcNow;

        // Hash new password
        string newHash = _passwordService.HashPassword(request.NewPassword);
        if (user.Credential == null)
        {
            user.Credential = new UserCredential { UserId = user.Id };
            _context.UserCredentials.Add(user.Credential);
        }

        user.Credential.PasswordHash = newHash;
        user.Credential.SecurityStamp = Guid.NewGuid().ToString("N");
        user.Credential.PasswordUpdatedAt = DateTimeOffset.UtcNow;
        user.MustChangePassword = false;

        _context.PasswordHistories.Add(new PasswordHistory
        {
            UserId = user.Id,
            PasswordHash = newHash
        });

        // Revoke all existing sessions upon password reset
        await RevokeAllUserSessionsInternalAsync(user.Id, "Password reset", cancellationToken);

        _context.SecurityEvents.Add(new SecurityEvent
        {
            UserId = user.Id,
            EventType = SecurityEventType.PasswordResetCompleted,
            Description = "Password reset successfully completed.",
            IpAddress = ipAddress,
            UserAgent = userAgent
        });

        await _context.SaveChangesAsync(cancellationToken);
        await _emailService.SendPasswordChangedNotificationAsync(user.Email, $"{user.FirstName} {user.LastName}", cancellationToken);

        return ApiResponse.OkMessage("Password has been reset successfully. Please log in with your new password.");
    }

    public async Task<ApiResponse> ChangePasswordAsync(
        ChangePasswordRequest request,
        long userId,
        string ipAddress,
        string userAgent,
        CancellationToken cancellationToken = default)
    {
        if (request.NewPassword != request.ConfirmNewPassword)
        {
            return ApiResponse.FailMessage("New passwords do not match.", 400);
        }

        if (request.NewPassword.Length < 8)
        {
            return ApiResponse.FailMessage("Password must be at least 8 characters long.", 400);
        }

        var user = await _context.Users
            .Include(u => u.Credential)
            .Include(u => u.PasswordHistories)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null || user.Credential == null)
        {
            return ApiResponse.FailMessage("User not found.", 404);
        }

        if (!_passwordService.VerifyPassword(request.CurrentPassword, user.Credential.PasswordHash))
        {
            return ApiResponse.FailMessage("Current password is incorrect.", 400);
        }

        // Prevent password reuse
        foreach (var history in user.PasswordHistories.OrderByDescending(h => h.CreatedAt).Take(5))
        {
            if (_passwordService.VerifyPassword(request.NewPassword, history.PasswordHash))
            {
                return ApiResponse.FailMessage("You cannot reuse any of your last 5 passwords.", 400);
            }
        }

        string newHash = _passwordService.HashPassword(request.NewPassword);
        user.Credential.PasswordHash = newHash;
        user.Credential.SecurityStamp = Guid.NewGuid().ToString("N");
        user.Credential.PasswordUpdatedAt = DateTimeOffset.UtcNow;
        user.MustChangePassword = false;

        _context.PasswordHistories.Add(new PasswordHistory
        {
            UserId = user.Id,
            PasswordHash = newHash
        });

        _context.SecurityEvents.Add(new SecurityEvent
        {
            UserId = user.Id,
            EventType = SecurityEventType.PasswordChanged,
            Description = "Password changed by user in account settings.",
            IpAddress = ipAddress,
            UserAgent = userAgent
        });

        await _context.SaveChangesAsync(cancellationToken);
        await _emailService.SendPasswordChangedNotificationAsync(user.Email, $"{user.FirstName} {user.LastName}", cancellationToken);

        return ApiResponse.OkMessage("Password updated successfully.");
    }

    public async Task<ApiResponse> FirstLoginSetupAsync(
        FirstLoginSetupRequest request,
        string ipAddress,
        string userAgent,
        CancellationToken cancellationToken = default)
    {
        if (request.NewPassword != request.ConfirmNewPassword)
        {
            return ApiResponse.FailMessage("Passwords do not match.", 400);
        }

        string normalized = request.Email.Trim().ToUpperInvariant();
        var user = await _context.Users
            .Include(u => u.Credential)
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalized, cancellationToken);

        if (user == null)
        {
            return ApiResponse.FailMessage("Invalid request.", 400);
        }

        string tokenHash = _tokenService.HashToken(request.Token);
        var tokenRecord = await _context.PasswordResetTokens
            .Where(t => t.UserId == user.Id && !t.IsUsed && t.ExpiresAt > DateTimeOffset.UtcNow)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (tokenRecord == null)
        {
            return ApiResponse.FailMessage("Invitation token is invalid or has expired.", 400);
        }

        tokenRecord.IsUsed = true;
        tokenRecord.UsedAt = DateTimeOffset.UtcNow;

        string newHash = _passwordService.HashPassword(request.NewPassword);
        if (user.Credential == null)
        {
            user.Credential = new UserCredential { UserId = user.Id };
            _context.UserCredentials.Add(user.Credential);
        }

        user.Credential.PasswordHash = newHash;
        user.Credential.SecurityStamp = Guid.NewGuid().ToString("N");
        user.Credential.PasswordUpdatedAt = DateTimeOffset.UtcNow;
        user.Status = AccountStatus.Active;
        user.EmailConfirmed = true;
        user.MustChangePassword = false;

        _context.PasswordHistories.Add(new PasswordHistory
        {
            UserId = user.Id,
            PasswordHash = newHash
        });

        _context.SecurityEvents.Add(new SecurityEvent
        {
            UserId = user.Id,
            EventType = SecurityEventType.FirstLoginCompleted,
            Description = "First login account setup completed.",
            IpAddress = ipAddress,
            UserAgent = userAgent
        });

        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponse.OkMessage("Account setup completed successfully. You may now log in.");
    }

    public async Task<ApiResponse> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken = default)
    {
        string normalized = request.Email.Trim().ToUpperInvariant();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalized, cancellationToken);
        if (user == null)
            return ApiResponse.FailMessage("Invalid verification request.", 400);

        string tokenHash = _tokenService.HashToken(request.Token);
        var record = await _context.EmailVerificationTokens
            .FirstOrDefaultAsync(t => t.UserId == user.Id && t.TokenHash == tokenHash && !t.IsUsed && t.ExpiresAt > DateTimeOffset.UtcNow, cancellationToken);

        if (record == null)
            return ApiResponse.FailMessage("Verification token is invalid or expired.", 400);

        record.IsUsed = true;
        record.UsedAt = DateTimeOffset.UtcNow;
        user.EmailConfirmed = true;

        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponse.OkMessage("Email verified successfully.");
    }

    public async Task<ApiResponse<List<UserSessionDto>>> GetSessionsAsync(
        long userId,
        Guid? currentSessionId,
        CancellationToken cancellationToken = default)
    {
        var sessions = await _context.UserSessions
            .Where(s => s.UserId == userId && !s.IsRevoked && s.ExpiresAt > DateTimeOffset.UtcNow)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new UserSessionDto(
                s.Id,
                s.IpAddress,
                s.UserAgent,
                s.DeviceInfo,
                s.CreatedAt,
                s.ExpiresAt,
                currentSessionId.HasValue && s.Id == currentSessionId.Value
            ))
            .ToListAsync(cancellationToken);

        return ApiResponse<List<UserSessionDto>>.Ok(sessions);
    }

    public async Task<ApiResponse> RevokeSessionAsync(
        Guid sessionId,
        long userId,
        string ipAddress,
        string userAgent,
        CancellationToken cancellationToken = default)
    {
        var session = await _context.UserSessions
            .Include(s => s.RefreshTokens)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId, cancellationToken);

        if (session == null)
            return ApiResponse.FailMessage("Session not found.", 404);

        session.IsRevoked = true;
        session.RevokedAt = DateTimeOffset.UtcNow;
        session.RevokedReason = "Revoked by user";

        foreach (var rt in session.RefreshTokens)
        {
            rt.IsRevoked = true;
            rt.RevokedAt = DateTimeOffset.UtcNow;
        }

        _context.SecurityEvents.Add(new SecurityEvent
        {
            UserId = userId,
            EventType = SecurityEventType.SessionRevoked,
            Description = $"Session {sessionId} revoked by user.",
            IpAddress = ipAddress,
            UserAgent = userAgent
        });

        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponse.OkMessage("Session revoked successfully.");
    }

    public async Task<ApiResponse> RevokeAllSessionsAsync(
        long userId,
        string ipAddress,
        string userAgent,
        CancellationToken cancellationToken = default)
    {
        await RevokeAllUserSessionsInternalAsync(userId, "Revoked by user", cancellationToken);

        _context.SecurityEvents.Add(new SecurityEvent
        {
            UserId = userId,
            EventType = SecurityEventType.SessionRevoked,
            Description = "All active sessions revoked by user.",
            IpAddress = ipAddress,
            UserAgent = userAgent
        });

        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponse.OkMessage("All sessions have been revoked successfully.");
    }

    public async Task<ApiResponse<UserProfileDto>> GetProfileAsync(long userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r!.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .Include(u => u.PropertyAssignments)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            return ApiResponse<UserProfileDto>.Fail("User not found.", 404);

        var roles = user.UserRoles.Select(ur => ur.Role!.Name).Distinct().ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role!.RolePermissions)
            .Select(rp => rp.Permission!.Code)
            .Distinct()
            .ToList();

        var profile = BuildUserProfileDto(user, roles, permissions);
        return ApiResponse<UserProfileDto>.Ok(profile);
    }

    private async Task RevokeAllUserSessionsInternalAsync(long userId, string reason, CancellationToken cancellationToken)
    {
        var activeSessions = await _context.UserSessions
            .Include(s => s.RefreshTokens)
            .Where(s => s.UserId == userId && !s.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var s in activeSessions)
        {
            s.IsRevoked = true;
            s.RevokedAt = DateTimeOffset.UtcNow;
            s.RevokedReason = reason;

            foreach (var rt in s.RefreshTokens)
            {
                rt.IsRevoked = true;
                rt.RevokedAt = DateTimeOffset.UtcNow;
            }
        }
    }

    private async Task RecordLoginAttemptAsync(
        string usernameOrEmail,
        string ipAddress,
        string userAgent,
        bool isSuccess,
        string? failureReason,
        CancellationToken cancellationToken)
    {
        _context.LoginAttempts.Add(new LoginAttempt
        {
            UsernameOrEmail = usernameOrEmail,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            IsSuccess = isSuccess,
            FailureReason = failureReason,
            AttemptedAt = DateTimeOffset.UtcNow
        });
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static UserProfileDto BuildUserProfileDto(User user, List<string> roles, List<string> permissions)
    {
        var defaultProp = user.PropertyAssignments.FirstOrDefault(p => p.IsDefault)?.PropertyId
            ?? user.PropertyAssignments.FirstOrDefault()?.PropertyId;

        return new UserProfileDto(
            user.Id,
            user.PublicId,
            user.Username,
            user.Email,
            user.FirstName,
            user.LastName,
            user.PhoneNumber,
            user.EmployeeId,
            user.DepartmentId,
            user.DesignationId,
            user.Status.ToString(),
            user.MustChangePassword,
            roles,
            permissions,
            defaultProp,
            user.PropertyAssignments.Select(p => p.PropertyId).ToList()
        );
    }
}
