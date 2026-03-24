using Microsoft.EntityFrameworkCore;
using ProductWorkflowService.Entities;

namespace ProductWorkflowService.Data
{
    public class WorkflowDbContext : DbContext
    {
        public WorkflowDbContext(DbContextOptions<WorkflowDbContext> options) : base(options) { }

        public DbSet<ProductPricing> Pricings { get; set; }
        public DbSet<ProductInventory> Inventories { get; set; }
        public DbSet<ProductApproval> Approvals { get; set; }
    }
}
