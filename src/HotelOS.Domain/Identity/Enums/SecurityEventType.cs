namespace HotelOS.Domain.Identity.Enums;

public enum SecurityEventType
{
    LoginSuccess = 1,
    LoginFailed = 2,
    Logout = 3,
    PasswordChanged = 4,
    PasswordResetRequested = 5,
    PasswordResetCompleted = 6,
    AccountLocked = 7,
    AccountUnlocked = 8,
    SessionRevoked = 9,
    FirstLoginCompleted = 10,
    TokenRefreshed = 11
}
