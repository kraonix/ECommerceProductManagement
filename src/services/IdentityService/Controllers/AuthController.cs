using IdentityService.Data;
using IdentityService.DTOs;
using IdentityService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IdentityService.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [EnableRateLimiting("auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly IdentityDbContext _db;

        public AuthController(AuthService authService, IdentityDbContext db)
        {
            _authService = authService;
            _db = db;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] SignupRequestDto dto)
        {
            return Ok(await _authService.SignupAsync(dto));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            return Ok(await _authService.LoginAsync(dto));
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto dto)
        {
            return Ok(await _authService.RefreshTokenAsync(dto));
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
        {
            // Note: Normally emails this, but mapped to direct JSON response for UX testing
            var token = await _authService.ForgotPasswordAsync(dto.Email);
            return Ok(new { message = "Password reset instructions initialized", resetToken = token });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto)
        {
            await _authService.ResetPasswordAsync(dto.Token, dto.NewPassword);
            return Ok(new { message = "Password has been successfully reset" });
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            await _authService.ChangePasswordAsync(userId, dto.CurrentPassword, dto.NewPassword);
            return Ok(new { message = "Your password was effectively updated." });
        }

        [Authorize]
        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            await _authService.RevokeTokenAsync(userId);
            return Ok(new { message = "Token successfully revoked permanently." });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var profile = await _authService.GetUserProfileAsync(userId);
            return Ok(profile);
        }

        /// <summary>
        /// DB diagnostic — returns user count and registered emails.
        /// Admin only. GET /api/auth/db-status
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("db-status")]
        public async Task<IActionResult> DbStatus()
        {
            try
            {
                var canConnect = await _db.Database.CanConnectAsync();
                if (!canConnect)
                    return StatusCode(503, new { connected = false, message = "Cannot connect to database." });

                var users = await _db.Users
                    .Select(u => new { u.UserId, u.Email, u.Role, u.IsActive, u.CreatedAt })
                    .OrderBy(u => u.CreatedAt)
                    .ToListAsync();

                var appliedMigrations = await _db.Database.GetAppliedMigrationsAsync();

                return Ok(new
                {
                    connected        = true,
                    database         = _db.Database.GetDbConnection().Database,
                    userCount        = users.Count,
                    users,
                    appliedMigrations
                });
            }
            catch (Exception ex)
            {
                return StatusCode(503, new { connected = false, message = ex.Message });
            }
        }
    }
}
