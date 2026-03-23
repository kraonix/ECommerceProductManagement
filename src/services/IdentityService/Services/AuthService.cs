using IdentityService.Data;
using IdentityService.DTOs;
using IdentityService.Entities;
using Microsoft.EntityFrameworkCore;

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
                throw new InvalidOperationException("Email already registered.");

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

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var token = _jwtService.GenerateToken(user);
            return new AuthResponseDto
            {
                Token = token,
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
                throw new UnauthorizedAccessException("Invalid credentials or account inactive.");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid credentials.");

            var token = _jwtService.GenerateToken(user);
            return new AuthResponseDto
            {
                Token = token,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                ExpiresAt = DateTime.UtcNow.AddHours(8)
            };
        }
    }
}
