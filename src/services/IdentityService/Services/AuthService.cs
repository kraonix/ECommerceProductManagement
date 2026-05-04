using IdentityService.Data;
using IdentityService.DTOs;
using IdentityService.Entities;
using IdentityService.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace IdentityService.Services
{
    public class AuthService
    {
        private readonly IdentityDbContext _db;
        private readonly JwtService _jwtService;

        public AuthService(IdentityDbContext db, JwtService jwtService)
        {
            _db = db;
            _jwtService = jwtService;
        }

        public async Task<AuthResponseDto> SignupAsync(SignupRequestDto dto)
        {
            var exists = await _db.Users.AnyAsync(u => u.Email == dto.Email);
            if (exists)
                throw new EmailAlreadyRegisteredException(dto.Email);

            // Normalize role to PascalCase so "admin", "ADMIN", "Admin" all work
            var normalizedRole = dto.Role?.Trim() ?? string.Empty;
            normalizedRole = normalizedRole switch
            {
                var r when r.Equals("admin", StringComparison.OrdinalIgnoreCase)            => "Admin",
                var r when r.Equals("productmanager", StringComparison.OrdinalIgnoreCase)   => "ProductManager",
                var r when r.Equals("contentexecutive", StringComparison.OrdinalIgnoreCase) => "ContentExecutive",
                var r when r.Equals("customer", StringComparison.OrdinalIgnoreCase)         => "Customer",
                _ => normalizedRole
            };

            var allowedRoles = new[] { "Admin", "ProductManager", "ContentExecutive", "Customer" };
            if (!allowedRoles.Contains(normalizedRole))
                throw new ArgumentException($"Invalid role '{dto.Role}'. Allowed values: Admin, ProductManager, ContentExecutive, Customer.");

            var user = new User
            {
                FullName = dto.FullName.Trim(),
                Email = dto.Email.Trim().ToLowerInvariant(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = normalizedRole
            };

            var refreshToken = _jwtService.GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var token = _jwtService.GenerateToken(user);
            return new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                ExpiresAt = DateTime.UtcNow.AddHours(8)
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || !user.IsActive)
                throw new InvalidCredentialsException();

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new InvalidCredentialsException();

            var refreshToken = _jwtService.GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _db.SaveChangesAsync();

            var token = _jwtService.GenerateToken(user);
            return new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                ExpiresAt = DateTime.UtcNow.AddHours(8)
            };
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.RefreshToken == dto.RefreshToken);

            // Reuse detection: if the submitted token doesn't match any user but the
            // access token IS valid, a stolen token may have been used after rotation.
            // In that case we revoke the family by clearing the stored token.
            if (user == null)
            {
                // Try to identify the user from the access token to revoke their session
                try
                {
                    var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                    if (handler.CanReadToken(dto.Token))
                    {
                        var jwt = handler.ReadJwtToken(dto.Token);
                        var userIdClaim = jwt.Claims
                            .FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier
                                              || c.Type == "nameid"
                                              || c.Type == "sub");
                        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var uid))
                        {
                            var staleUser = await _db.Users.FindAsync(uid);
                            if (staleUser != null)
                            {
                                // Revoke the entire session — token reuse detected
                                staleUser.RefreshToken = null;
                                staleUser.RefreshTokenExpiryTime = null;
                                await _db.SaveChangesAsync();
                            }
                        }
                    }
                }
                catch { /* best-effort — still throw below */ }

                throw new TokenValidationException("Invalid or expired refresh token.");
            }

            if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                // Expired — clear it and force re-login
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                await _db.SaveChangesAsync();
                throw new TokenValidationException("Refresh token has expired. Please log in again.");
            }

            // Rotate: issue a new refresh token and invalidate the old one immediately
            var newRefreshToken = _jwtService.GenerateRefreshToken();
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _db.SaveChangesAsync();

            var newToken = _jwtService.GenerateToken(user);
            return new AuthResponseDto
            {
                Token = newToken,
                RefreshToken = newRefreshToken,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                ExpiresAt = DateTime.UtcNow.AddHours(8)
            };
        }

        public async Task<string> ForgotPasswordAsync(string email)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                throw new UserNotFoundException(email);

            var resetToken = _jwtService.GenerateRefreshToken();
            user.PasswordResetToken = resetToken;
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
            
            await _db.SaveChangesAsync();
            return resetToken;
        }

        public async Task ResetPasswordAsync(string token, string newPassword)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == token);
            if (user == null || user.PasswordResetTokenExpiry <= DateTime.UtcNow)
                throw new TokenValidationException("Invalid or expired password reset token.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            await _db.SaveChangesAsync();
        }

        public async Task ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) throw new UserNotFoundException(userId.ToString());
            
            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
                throw new InvalidCredentialsException();

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _db.SaveChangesAsync();
        }

        public async Task RevokeTokenAsync(int userId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) throw new UserNotFoundException(userId.ToString());

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _db.SaveChangesAsync();
        }

        public async Task<object> GetUserProfileAsync(int userId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) throw new UserNotFoundException(userId.ToString());
            
            return new { user.FullName, user.Email, user.Role, user.CreatedAt };
        }
    }
}
