using IdentityService.Data;
using IdentityService.DTOs;
using IdentityService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;

namespace IdentityService.Tests
{
    [TestFixture]
    public class AuthServiceTests
    {
        private IdentityDbContext _db = null!;
        private JwtService _jwtService = null!;
        private AuthService _authService = null!;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<IdentityDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new IdentityDbContext(options);

            var configData = new Dictionary<string, string?>
            {
                { "JwtSettings:SecretKey", "ECommerceTestSuperSecretKey_2024!@#$" },
                { "JwtSettings:Issuer", "TestIssuer" },
                { "JwtSettings:Audience", "TestAudience" },
                { "JwtSettings:ExpiryHours", "8" }
            };
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            _jwtService = new JwtService(config);
            _authService = new AuthService(_db, _jwtService);
        }

        [TearDown]
        public void TearDown() => _db.Dispose();

        [Test]
        public async Task Signup_ValidUser_ReturnsToken()
        {
            var dto = new SignupRequestDto
            {
                FullName = "Test Admin",
                Email = "admin@test.com",
                Password = "Admin@12345",
                Role = "Admin"
            };
            var result = await _authService.SignupAsync(dto);
            Assert.That(result.Token, Is.Not.Empty);
            Assert.That(result.Email, Is.EqualTo("admin@test.com"));
        }

        [Test]
        public async Task Signup_DuplicateEmail_ThrowsException()
        {
            var dto = new SignupRequestDto
            {
                FullName = "User One",
                Email = "dup@test.com",
                Password = "Pass@1234",
                Role = "ProductManager"
            };
            await _authService.SignupAsync(dto);
            Assert.ThrowsAsync<InvalidOperationException>(
                () => _authService.SignupAsync(dto));
        }

        [Test]
        public async Task Login_ValidCredentials_ReturnsToken()
        {
            var signup = new SignupRequestDto
            {
                FullName = "Login User",
                Email = "login@test.com",
                Password = "Login@1234",
                Role = "ProductManager"
            };
            await _authService.SignupAsync(signup);

            var login = new LoginRequestDto
            {
                Email = "login@test.com",
                Password = "Login@1234"
            };
            var result = await _authService.LoginAsync(login);
            Assert.That(result.Token, Is.Not.Empty);
        }

        [Test]
        public async Task Login_WrongPassword_ThrowsUnauthorized()
        {
            var signup = new SignupRequestDto
            {
                FullName = "Login User",
                Email = "wrong@test.com",
                Password = "Correct@1234",
                Role = "ProductManager"
            };
            await _authService.SignupAsync(signup);

            var login = new LoginRequestDto
            {
                Email = "wrong@test.com",
                Password = "WrongPassword"
            };
            Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authService.LoginAsync(login));
        }

        [Test]
        public void Signup_InvalidRole_ThrowsArgumentException()
        {
            var dto = new SignupRequestDto
            {
                FullName = "Bad Role",
                Email = "badrole@test.com",
                Password = "Pass@1234",
                Role = "SuperUser"
            };
            Assert.ThrowsAsync<ArgumentException>(
                () => _authService.SignupAsync(dto));
        }
    }
}
