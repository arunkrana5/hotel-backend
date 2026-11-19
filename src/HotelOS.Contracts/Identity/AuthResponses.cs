namespace HotelOS.Contracts.Identity;

public record AuthTokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt,
    UserProfileDto User
);

public record UserProfileDto(
    long Id,
    Guid PublicId,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? EmployeeId,
    long? DepartmentId,
    long? DesignationId,
    string Status,
    bool MustChangePassword,
    List<string> Roles,
    List<string> Permissions,
    long? DefaultPropertyId,
    List<long> AssignedPropertyIds
);

public record UserSessionDto(
    Guid SessionId,
    string IpAddress,
    string UserAgent,
    string? DeviceInfo,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    bool IsCurrent
);

public record ResetTokenVerificationResponse(
    bool IsValid,
    string Email,
    string Message
);
