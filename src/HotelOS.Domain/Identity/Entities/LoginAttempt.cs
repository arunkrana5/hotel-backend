namespace HotelOS.Domain.Identity.Entities;

public class LoginAttempt
{
    public long Id { get; set; }
    public string UsernameOrEmail { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string? FailureReason { get; set; }
    public DateTimeOffset AttemptedAt { get; set; } = DateTimeOffset.UtcNow;
}
