using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Identity;
using HotelOS.Domain.Identity.Entities;
using HotelOS.Domain.Identity.Enums;
using HotelOS.Infrastructure.Persistence;
using HotelOS.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HotelOS.UnitTests;

public class AuthenticationTests
{
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;

    public AuthenticationTests()
    {
        _passwordService = new PasswordService();

        var inMemorySettings = new Dictionary<string, string?>
        {
            {"Jwt:SecretKey", "Unit_Test_Super_Secret_Key_HotelOS_2026_Secure_!#*"},
            {"Jwt:Issuer", "HotelOS.Api"},
            {"Jwt:Audience", "HotelOS.Client"},
            {"Jwt:ExpiryMinutes", "60"},
            {"Jwt:RefreshTokenExpiryDays", "7"}
        };
        _configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
        _tokenService = new TokenService(_configuration);
    }

    [Fact]
    public void PasswordService_Should_Hash_And_Verify_Correctly()
    {
        string raw = "SecureAdmin@123!";
        string hash = _passwordService.HashPassword(raw);

        Assert.NotEmpty(hash);
        Assert.True(_passwordService.VerifyPassword(raw, hash));
        Assert.False(_passwordService.VerifyPassword("WrongPassword", hash));
    }

    [Fact]
    public void TokenService_Should_Generate_Valid_Crypto_Tokens()
    {
        var (rawToken, tokenHash, expiresAt) = _tokenService.GenerateCryptoToken(TimeSpan.FromMinutes(15));

        Assert.NotEmpty(rawToken);
        Assert.NotEmpty(tokenHash);
        Assert.True(expiresAt > DateTimeOffset.UtcNow);

        string recomputedHash = _tokenService.HashToken(rawToken);
        Assert.Equal(tokenHash, recomputedHash);
    }

    [Fact]
    public async Task AuthService_Login_Should_Fail_With_Wrong_Password_And_Lock_After_Max_Attempts()
    {
        var options = new DbContextOptionsBuilder<HotelOSDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new HotelOSDbContext(options);
        var emailService = new EmailService(NullLogger<EmailService>.Instance, _configuration);
        var authService = new AuthService(context, _passwordService, _tokenService, emailService, NullLogger<AuthService>.Instance);

        // Seed test user
        var user = new User
        {
            Username = "testuser",
            NormalizedUsername = "TESTUSER",
            Email = "test@hotelos.internal",
            NormalizedEmail = "TEST@HOTELOS.INTERNAL",
            FirstName = "Test",
            LastName = "User",
            Status = AccountStatus.Active
        };
        user.Credential = new UserCredential
        {
            PasswordHash = _passwordService.HashPassword("CorrectPassword123!"),
            SecurityStamp = Guid.NewGuid().ToString("N")
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // 1. Successful login
        var successResult = await authService.LoginAsync(
            new LoginRequest("testuser", "CorrectPassword123!"),
            "127.0.0.1", "TestAgent");

        Assert.True(successResult.Success);
        Assert.NotNull(successResult.Data?.AccessToken);
        Assert.NotNull(successResult.Data?.RefreshToken);

        // 2. 5 Consecutive failed attempts
        for (int i = 0; i < 4; i++)
        {
            var fail = await authService.LoginAsync(
                new LoginRequest("testuser", "BadPassword"),
                "127.0.0.1", "TestAgent");
            Assert.False(fail.Success);
            Assert.Equal(401, fail.StatusCode);
        }

        // 5th attempt triggers lockout
        var lockoutResult = await authService.LoginAsync(
            new LoginRequest("testuser", "BadPassword"),
            "127.0.0.1", "TestAgent");

        Assert.False(lockoutResult.Success);

        // Next attempt (even with correct password) should be locked
        var attemptWhileLocked = await authService.LoginAsync(
            new LoginRequest("testuser", "CorrectPassword123!"),
            "127.0.0.1", "TestAgent");

        Assert.False(attemptWhileLocked.Success);
        Assert.Equal(423, attemptWhileLocked.StatusCode);
        Assert.Contains("locked", attemptWhileLocked.Message.ToLowerInvariant());
    }

    [Fact]
    public async Task AuthService_ResetPassword_Should_Prevent_Password_Reuse()
    {
        var options = new DbContextOptionsBuilder<HotelOSDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new HotelOSDbContext(options);
        var emailService = new EmailService(NullLogger<EmailService>.Instance, _configuration);
        var authService = new AuthService(context, _passwordService, _tokenService, emailService, NullLogger<AuthService>.Instance);

        string oldPass = "OldPassword123!";
        string oldHash = _passwordService.HashPassword(oldPass);

        var user = new User
        {
            Username = "reuseuser",
            NormalizedUsername = "REUSEUSER",
            Email = "reuse@hotelos.internal",
            NormalizedEmail = "REUSE@HOTELOS.INTERNAL",
            FirstName = "Reuse",
            LastName = "User",
            Status = AccountStatus.Active
        };
        user.Credential = new UserCredential
        {
            PasswordHash = oldHash,
            SecurityStamp = Guid.NewGuid().ToString("N")
        };
        context.Users.Add(user);
        context.PasswordHistories.Add(new PasswordHistory { UserId = user.Id, PasswordHash = oldHash });

        var (rawToken, tokenHash, expiresAt) = _tokenService.GenerateCryptoToken(TimeSpan.FromMinutes(15));
        context.PasswordResetTokens.Add(new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt
        });

        await context.SaveChangesAsync();

        // Try resetting to same old password
        var reuseResult = await authService.ResetPasswordAsync(
            new ResetPasswordRequest("reuse@hotelos.internal", rawToken, oldPass, oldPass),
            "127.0.0.1", "TestAgent");

        Assert.False(reuseResult.Success);
        Assert.Contains("cannot reuse", reuseResult.Message.ToLowerInvariant());
    }
}
