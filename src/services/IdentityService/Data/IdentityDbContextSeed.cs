using IdentityService.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Data
{
    /// <summary>
    /// Seeds default accounts on first run so the system is never left with zero users.
    /// Only inserts if the email doesn't already exist — safe to call on every startup.
    /// </summary>
    public static class IdentityDbContextSeed
    {
        public static async Task SeedAsync(IdentityDbContext db)
        {
            // Default accounts — change passwords immediately in production
            var defaults = new[]
            {
                new { FullName = "System Admin",       Email = "admin@ecommerce.local",    Password = "Admin@123",   Role = "Admin"            },
                new { FullName = "Product Manager",    Email = "pm@ecommerce.local",       Password = "Manager@123", Role = "ProductManager"   },
                new { FullName = "Content Executive",  Email = "content@ecommerce.local",  Password = "Content@123", Role = "ContentExecutive" },
                new { FullName = "Demo Customer",      Email = "customer@ecommerce.com",   Password = "Customer@123",Role = "Customer"         },
            };

            bool anyAdded = false;

            foreach (var seed in defaults)
            {
                var exists = await db.Users.AnyAsync(u => u.Email == seed.Email);
                if (!exists)
                {
                    db.Users.Add(new User
                    {
                        FullName              = seed.FullName,
                        Email                 = seed.Email,
                        PasswordHash          = BCrypt.Net.BCrypt.HashPassword(seed.Password),
                        Role                  = seed.Role,
                        IsActive              = true,
                        CreatedAt             = DateTime.UtcNow,
                        RefreshToken          = null,
                        RefreshTokenExpiryTime = null
                    });
                    anyAdded = true;
                }
            }

            if (anyAdded)
                await db.SaveChangesAsync();
        }
    }
}
