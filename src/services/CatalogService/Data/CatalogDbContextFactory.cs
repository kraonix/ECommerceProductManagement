using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CatalogService.Data
{
    public class CatalogDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
    {
        public CatalogDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CatalogDbContext>();
            optionsBuilder.UseSqlServer("Server=localhost;Database=ECommerce_CatalogDb;Trusted_Connection=True;TrustServerCertificate=True;");
            return new CatalogDbContext(optionsBuilder.Options);
        }
    }
}
