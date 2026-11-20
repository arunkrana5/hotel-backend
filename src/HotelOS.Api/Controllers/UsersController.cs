using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Common;
using HotelOS.Domain.Identity.Entities;
using HotelOS.Domain.Identity.Enums;
using HotelOS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelOS.Api.Controllers;

public record CreateUserRequest(
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string Password,
    string? PhoneNumber,
    string? EmployeeId,
    List<long> RoleIds,
    long? DepartmentId = null
);

public record UpdateUserStatusRequest(string Status);

public record AdminResetPasswordRequest(string NewPassword);

public record UserItemDto(
    long Id,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    string? PhoneNumber,
    string? EmployeeId,
    string Status,
    List<string> Roles,
    DateTimeOffset? LastLoginAt,
    DateTimeOffset CreatedAt
);

public record RoleItemDto(
    long Id,
    string Name,
    string NormalizedName,
    string Description,
    bool IsSystemRole,
    int UsersCount,
    List<string> Permissions
);

[ApiController]
[Route("api/v1")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly HotelOSDbContext _context;
    private readonly IPasswordService _passwordService;
    private readonly ICurrentUserService _currentUser;

    public UsersController(
        HotelOSDbContext context,
        IPasswordService passwordService,
        ICurrentUserService currentUser)
    {
        _context = context;
        _passwordService = passwordService;
        _currentUser = currentUser;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] string? role = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Where(u => !u.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            string s = search.Trim().ToUpperInvariant();
            query = query.Where(u =>
                u.NormalizedUsername.Contains(s) ||
                u.NormalizedEmail.Contains(s) ||
                u.FirstName.ToUpper().Contains(s) ||
                u.LastName.ToUpper().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<AccountStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(u => u.Status == parsedStatus);
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            string lowerRole = role.ToLower();
            query = query.Where(u => u.UserRoles.Any(ur => ur.Role.Name.ToLower() == lowerRole));
        }

        int totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserItemDto(
                u.Id,
                u.Username,
                u.Email,
                u.FirstName,
                u.LastName,
                u.FullName,
                u.PhoneNumber,
                u.EmployeeId,
                u.Status.ToString(),
                u.UserRoles.Select(ur => ur.Role.Name).ToList(),
                u.LastLoginAt,
                u.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<PagedResult<UserItemDto>>.Ok(
            PagedResult<UserItemDto>.Create(items, totalCount, page, pageSize)));
    }

    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUserById(long id, CancellationToken cancellationToken)
    {
        var u = await _context.Users
            .AsNoTracking()
            .Include(x => x.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (u == null)
            return NotFound(ApiResponse<UserItemDto>.Fail("User not found"));

        var dto = new UserItemDto(
            u.Id,
            u.Username,
            u.Email,
            u.FirstName,
            u.LastName,
            u.FullName,
            u.PhoneNumber,
            u.EmployeeId,
            u.Status.ToString(),
            u.UserRoles.Select(ur => ur.Role.Name).ToList(),
            u.LastLoginAt,
            u.CreatedAt
        );

        return Ok(ApiResponse<UserItemDto>.Ok(dto));
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(ApiResponse<UserItemDto>.Fail("Username, email, and password are required."));
        }

        string normUsername = request.Username.Trim().ToUpperInvariant();
        string normEmail = request.Email.Trim().ToUpperInvariant();

        if (await _context.Users.AnyAsync(u => u.NormalizedUsername == normUsername, cancellationToken))
        {
            return BadRequest(ApiResponse<UserItemDto>.Fail("Username is already in use."));
        }

        if (await _context.Users.AnyAsync(u => u.NormalizedEmail == normEmail, cancellationToken))
        {
            return BadRequest(ApiResponse<UserItemDto>.Fail("Email address is already in use."));
        }

        string passwordHash = _passwordService.HashPassword(request.Password);

        var newUser = new User
        {
            Username = request.Username.Trim(),
            NormalizedUsername = normUsername,
            Email = request.Email.Trim(),
            NormalizedEmail = normEmail,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PhoneNumber = request.PhoneNumber,
            EmployeeId = request.EmployeeId,
            DepartmentId = request.DepartmentId,
            Status = AccountStatus.Active,
            EmailConfirmed = true,
            MustChangePassword = false,
            CreatedBy = _currentUser.Username ?? "SYSTEM",
            Credential = new UserCredential
            {
                PasswordHash = passwordHash,
                SecurityStamp = Guid.NewGuid().ToString("N"),
                PasswordUpdatedAt = DateTimeOffset.UtcNow
            }
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync(cancellationToken);

        // Assign roles
        if (request.RoleIds != null && request.RoleIds.Count > 0)
        {
            foreach (var roleId in request.RoleIds)
            {
                _context.UserRoles.Add(new UserRole
                {
                    UserId = newUser.Id,
                    RoleId = roleId,
                    AssignedBy = _currentUser.Username ?? "SYSTEM"
                });
            }
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Return user item
        var roles = await _context.UserRoles
            .Where(ur => ur.UserId == newUser.Id)
            .Select(ur => ur.Role.Name)
            .ToListAsync(cancellationToken);

        var dto = new UserItemDto(
            newUser.Id,
            newUser.Username,
            newUser.Email,
            newUser.FirstName,
            newUser.LastName,
            newUser.FullName,
            newUser.PhoneNumber,
            newUser.EmployeeId,
            newUser.Status.ToString(),
            roles,
            null,
            newUser.CreatedAt
        );

        return Ok(ApiResponse<UserItemDto>.Ok(dto, "User created successfully."));
    }

    [HttpPut("users/{id}/status")]
    public async Task<IActionResult> UpdateUserStatus(long id, [FromBody] UpdateUserStatusRequest request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, cancellationToken);
        if (user == null)
            return NotFound(ApiResponse<bool>.Fail("User not found"));

        if (!Enum.TryParse<AccountStatus>(request.Status, true, out var newStatus))
            return BadRequest(ApiResponse<bool>.Fail("Invalid account status. Valid options: Active, Inactive, Locked"));

        user.Status = newStatus;
        user.ModifiedAt = DateTimeOffset.UtcNow;
        user.ModifiedBy = _currentUser.Username ?? "SYSTEM";

        if (newStatus == AccountStatus.Active)
        {
            user.LockoutEnd = null;
            user.AccessFailedCount = 0;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Ok(ApiResponse<bool>.Ok(true, $"User status updated to {newStatus}."));
    }

    [HttpPost("users/{id}/reset-password")]
    public async Task<IActionResult> AdminResetPassword(long id, [FromBody] AdminResetPasswordRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
            return BadRequest(ApiResponse<bool>.Fail("New password must be at least 6 characters long."));

        var user = await _context.Users
            .Include(u => u.Credential)
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, cancellationToken);

        if (user == null)
            return NotFound(ApiResponse<bool>.Fail("User not found"));

        string newHash = _passwordService.HashPassword(request.NewPassword);
        if (user.Credential == null)
        {
            user.Credential = new UserCredential
            {
                UserId = user.Id,
                PasswordHash = newHash,
                SecurityStamp = Guid.NewGuid().ToString("N"),
                PasswordUpdatedAt = DateTimeOffset.UtcNow
            };
        }
        else
        {
            user.Credential.PasswordHash = newHash;
            user.Credential.SecurityStamp = Guid.NewGuid().ToString("N");
            user.Credential.PasswordUpdatedAt = DateTimeOffset.UtcNow;
        }

        user.MustChangePassword = false;
        user.ModifiedAt = DateTimeOffset.UtcNow;
        user.ModifiedBy = _currentUser.Username ?? "SYSTEM";

        _context.PasswordHistories.Add(new PasswordHistory
        {
            UserId = user.Id,
            PasswordHash = newHash
        });

        await _context.SaveChangesAsync(cancellationToken);
        return Ok(ApiResponse<bool>.Ok(true, "Password has been reset successfully."));
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles(CancellationToken cancellationToken)
    {
        var roles = await _context.Roles
            .AsNoTracking()
            .Include(r => r.UserRoles)
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .Where(r => !r.IsDeleted)
            .OrderBy(r => r.Id)
            .Select(r => new RoleItemDto(
                r.Id,
                r.Name,
                r.NormalizedName,
                r.Description,
                r.IsSystemRole,
                r.UserRoles.Count,
                r.RolePermissions.Select(rp => rp.Permission.Code).ToList()
            ))
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<List<RoleItemDto>>.Ok(roles));
    }
}
