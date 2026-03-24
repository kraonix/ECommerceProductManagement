using AdminReportingService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdminReportingService.Data
{
    public class AdminDbContext : DbContext
    {
        public AdminDbContext(DbContextOptions<AdminDbContext> options) : base(options) { }
        public DbSet<AuditLog> AuditLogs { get; set; }
    }
}
