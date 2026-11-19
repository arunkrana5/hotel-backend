using HotelOS.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HotelOS.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _configuration;

    public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public Task SendPasswordResetEmailAsync(string email, string token, string recipientName, CancellationToken cancellationToken = default)
    {
        string clientUrl = _configuration["App:ClientUrl"] ?? "http://localhost:3000";
        string resetLink = $"{clientUrl}/reset-password?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";

        _logger.LogInformation(
            "==========================================================\n" +
            "PASSWORD RESET EMAIL DISPATCHED\n" +
            "To: {Email} ({RecipientName})\n" +
            "Token: {Token}\n" +
            "Reset Link: {ResetLink}\n" +
            "==========================================================",
            email, recipientName, token, resetLink);

        return Task.CompletedTask;
    }

    public Task SendFirstLoginInvitationAsync(string email, string token, string recipientName, CancellationToken cancellationToken = default)
    {
        string clientUrl = _configuration["App:ClientUrl"] ?? "http://localhost:3000";
        string invitationLink = $"{clientUrl}/first-login?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";

        _logger.LogInformation(
            "==========================================================\n" +
            "FIRST LOGIN ONBOARDING INVITATION DISPATCHED\n" +
            "To: {Email} ({RecipientName})\n" +
            "Token: {Token}\n" +
            "Setup Link: {InvitationLink}\n" +
            "==========================================================",
            email, recipientName, token, invitationLink);

        return Task.CompletedTask;
    }

    public Task SendPasswordChangedNotificationAsync(string email, string recipientName, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "==========================================================\n" +
            "SECURITY NOTIFICATION: Password has been successfully changed for {Email} ({RecipientName})\n" +
            "==========================================================",
            email, recipientName);

        return Task.CompletedTask;
    }
}
