using HotelOS.Contracts.Common;
using HotelOS.Contracts.Identity;

namespace HotelOS.Application.Common.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthTokenResponse>> LoginAsync(LoginRequest request, string ipAddress, string userAgent, CancellationToken cancellationToken = default);
    Task<ApiResponse<AuthTokenResponse>> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress, string userAgent, CancellationToken cancellationToken = default);
    Task<ApiResponse> LogoutAsync(string? refreshToken, long? userId, CancellationToken cancellationToken = default);
    Task<ApiResponse> ForgotPasswordAsync(ForgotPasswordRequest request, string ipAddress, string userAgent, CancellationToken cancellationToken = default);
    Task<ApiResponse<ResetTokenVerificationResponse>> VerifyResetTokenAsync(VerifyResetTokenRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse> ResetPasswordAsync(ResetPasswordRequest request, string ipAddress, string userAgent, CancellationToken cancellationToken = default);
    Task<ApiResponse> ChangePasswordAsync(ChangePasswordRequest request, long userId, string ipAddress, string userAgent, CancellationToken cancellationToken = default);
    Task<ApiResponse> FirstLoginSetupAsync(FirstLoginSetupRequest request, string ipAddress, string userAgent, CancellationToken cancellationToken = default);
    Task<ApiResponse> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<UserSessionDto>>> GetSessionsAsync(long userId, Guid? currentSessionId, CancellationToken cancellationToken = default);
    Task<ApiResponse> RevokeSessionAsync(Guid sessionId, long userId, string ipAddress, string userAgent, CancellationToken cancellationToken = default);
    Task<ApiResponse> RevokeAllSessionsAsync(long userId, string ipAddress, string userAgent, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserProfileDto>> GetProfileAsync(long userId, CancellationToken cancellationToken = default);
}
