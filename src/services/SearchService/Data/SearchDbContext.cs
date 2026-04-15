using Microsoft.EntityFrameworkCore;
using SearchService.Entities;

namespace SearchService.Data
{
    /// <summary>
    /// Read-only view into the CatalogDB — SearchService never writes to this DB.
    /// </summary>
    public class SearchDbContext : DbContext
    {
        public SearchDbContext(DbContextOptions<SearchDbContext> options) : base(options) { }

        public DbSet<ProductIndex> Products { get; set; }
        public DbSet<MediaAssetIndex> MediaAssets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Map to the CatalogDB tables (read-only)
            modelBuilder.Entity<ProductIndex>().ToTable("Products");
            modelBuilder.Entity<MediaAssetIndex>().ToTable("MediaAssets");

            modelBuilder.Entity<ProductIndex>()
                .HasMany(p => p.MediaAssets)
                .WithOne()
                .HasForeignKey(m => m.ProductId);
        }
    }
}
