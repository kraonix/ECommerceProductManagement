using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AdminReportingService.Data
{
    public class AdminDbContextFactory : IDesignTimeDbContextFactory<AdminDbContext>
    {
        public AdminDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AdminDbContext>();
            optionsBuilder.UseSqlServer("Server=localhost;Database=ECommerce_AdminDb;Trusted_Connection=True;TrustServerCertificate=True;");
            return new AdminDbContext(optionsBuilder.Options);
        }
    }
}
