namespace HotelOS.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string email, string token, string recipientName, CancellationToken cancellationToken = default);
    Task SendFirstLoginInvitationAsync(string email, string token, string recipientName, CancellationToken cancellationToken = default);
    Task SendPasswordChangedNotificationAsync(string email, string recipientName, CancellationToken cancellationToken = default);
}
