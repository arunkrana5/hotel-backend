using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Common;
using HotelOS.Contracts.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelOS.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUser;

    public AuthController(IAuthService authService, ICurrentUserService currentUser)
    {
        _authService = authService;
        _currentUser = currentUser;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(
            request,
            _currentUser.IpAddress,
            _currentUser.UserAgent,
            cancellationToken);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        // Set refresh token in HttpOnly cookie if preferred, while also returning in body for SPA
        SetRefreshTokenCookie(result.Data!.RefreshToken, result.Data.ExpiresAt);

        return Ok(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest? request, CancellationToken cancellationToken)
    {
        string? tokenStr = request?.RefreshToken;
        if (string.IsNullOrWhiteSpace(tokenStr))
        {
            tokenStr = Request.Cookies["refreshToken"];
        }

        if (string.IsNullOrWhiteSpace(tokenStr))
        {
            return Unauthorized(ApiResponse.FailMessage("Refresh token is required.", 401));
        }

        var result = await _authService.RefreshTokenAsync(
            new RefreshTokenRequest(tokenStr),
            _currentUser.IpAddress,
            _currentUser.UserAgent,
            cancellationToken);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        SetRefreshTokenCookie(result.Data!.RefreshToken, result.Data.ExpiresAt);

        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        string? token = Request.Cookies["refreshToken"];
        var result = await _authService.LogoutAsync(token, _currentUser.UserId, cancellationToken);
        Response.Cookies.Delete("refreshToken");
        return Ok(result);
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.ForgotPasswordAsync(
            request,
            _currentUser.IpAddress,
            _currentUser.UserAgent,
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("verify-reset-token")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyResetToken([FromBody] VerifyResetTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.VerifyResetTokenAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.ResetPasswordAsync(
            request,
            _currentUser.IpAddress,
            _currentUser.UserAgent,
            cancellationToken);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return Unauthorized(ApiResponse.FailMessage("Unauthorized.", 401));

        var result = await _authService.ChangePasswordAsync(
            request,
            _currentUser.UserId.Value,
            _currentUser.IpAddress,
            _currentUser.UserAgent,
            cancellationToken);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    [HttpPost("first-login")]
    [AllowAnonymous]
    public async Task<IActionResult> FirstLogin([FromBody] FirstLoginSetupRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.FirstLoginSetupAsync(
            request,
            _currentUser.IpAddress,
            _currentUser.UserAgent,
            cancellationToken);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    [HttpPost("verify-email")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.VerifyEmailAsync(request, cancellationToken);
        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    [HttpGet("sessions")]
    [Authorize]
    public async Task<IActionResult> GetSessions(CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return Unauthorized(ApiResponse.FailMessage("Unauthorized.", 401));

        var result = await _authService.GetSessionsAsync(
            _currentUser.UserId.Value,
            null,
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("revoke-session")]
    [Authorize]
    public async Task<IActionResult> RevokeSession([FromBody] RevokeSessionRequest request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return Unauthorized(ApiResponse.FailMessage("Unauthorized.", 401));

        var result = await _authService.RevokeSessionAsync(
            request.SessionId,
            _currentUser.UserId.Value,
            _currentUser.IpAddress,
            _currentUser.UserAgent,
            cancellationToken);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    [HttpPost("revoke-all-sessions")]
    [Authorize]
    public async Task<IActionResult> RevokeAllSessions(CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return Unauthorized(ApiResponse.FailMessage("Unauthorized.", 401));

        var result = await _authService.RevokeAllSessionsAsync(
            _currentUser.UserId.Value,
            _currentUser.IpAddress,
            _currentUser.UserAgent,
            cancellationToken);

        Response.Cookies.Delete("refreshToken");
        return Ok(result);
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return Unauthorized(ApiResponse.FailMessage("Unauthorized.", 401));

        var result = await _authService.GetProfileAsync(_currentUser.UserId.Value, cancellationToken);
        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    private void SetRefreshTokenCookie(string token, DateTimeOffset expiresAt)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = expiresAt.UtcDateTime
        };
        Response.Cookies.Append("refreshToken", token, cookieOptions);
    }
}
