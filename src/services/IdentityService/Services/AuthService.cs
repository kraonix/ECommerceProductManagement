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

            var allowedRoles = new[] { "Admin", "ProductManager", "ContentExecutive", "Customer" };
            if (!allowedRoles.Contains(dto.Role))
                throw new ArgumentException("Invalid role specified.");

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role
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
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
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
            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                throw new TokenValidationException("Invalid or expired refresh token.");
            
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
