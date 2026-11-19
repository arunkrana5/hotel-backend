namespace HotelOS.Contracts.Identity;

public record LoginRequest(
    string UsernameOrEmail,
    string Password,
    bool RememberMe = false,
    string? DeviceInfo = null
);

public record RefreshTokenRequest(
    string RefreshToken
);

public record ForgotPasswordRequest(
    string Email
);

public record VerifyResetTokenRequest(
    string Email,
    string Token
);

public record ResetPasswordRequest(
    string Email,
    string Token,
    string NewPassword,
    string ConfirmNewPassword
);

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword
);

public record FirstLoginSetupRequest(
    string Email,
    string Token,
    string NewPassword,
    string ConfirmNewPassword
);

public record VerifyEmailRequest(
    string Email,
    string Token
);

public record RevokeSessionRequest(
    Guid SessionId
);
