using AdminReportingService.Data;
using AdminReportingService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AdminReportingService.Tests
{
    [TestFixture]
    public class ReportingManagerTests
    {
        private AdminDbContext _db = null!;
        private ReportingManager _manager = null!;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<AdminDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            _db = new AdminDbContext(options);
            _manager = new ReportingManager(_db);
        }

        [TearDown]
        public void TearDown() => _db.Dispose();

        [Test]
        public async Task ExportDashboard_GeneratesFile() // TC13
        {
            var result = await _manager.ExportDashboardDataAsync();
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.GreaterThan(0));
        }

        [Test]
        public async Task AuditHistoryLoad_ReturnsChronologicalOrder() // TC14
        {
            _db.AuditLogs.Add(new AdminReportingService.Entities.AuditLog
            {
                ProductId = 1, Action = "Create",
                Timestamp = DateTime.UtcNow.AddMinutes(-10)
            });
            _db.AuditLogs.Add(new AdminReportingService.Entities.AuditLog
            {
                ProductId = 1, Action = "Publish",
                Timestamp = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            var logs = await _manager.GetProductAuditHistoryAsync(1);
            Assert.That(logs.Count(), Is.EqualTo(2));
            Assert.That(logs.First().Action, Is.EqualTo("Publish")); // Descending order
        }
    }
}
