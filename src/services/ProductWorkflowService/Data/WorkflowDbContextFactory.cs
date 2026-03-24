using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ProductWorkflowService.Data
{
    public class WorkflowDbContextFactory : IDesignTimeDbContextFactory<WorkflowDbContext>
    {
        public WorkflowDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<WorkflowDbContext>();
            optionsBuilder.UseSqlServer("Server=localhost;Database=ECommerce_WorkflowDb;Trusted_Connection=True;TrustServerCertificate=True;");
            return new WorkflowDbContext(optionsBuilder.Options);
        }
    }
}
