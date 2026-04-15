using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using ProductWorkflowService.Data;
using ProductWorkflowService.DTOs;
using ProductWorkflowService.Services;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ProductWorkflowService.Tests
{
    [TestFixture]
    public class WorkflowManagerTests
    {
        private WorkflowDbContext _db = null!;
        private WorkflowManager _service = null!;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<WorkflowDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            _db = new WorkflowDbContext(options);

            var mockClientFactory = new Mock<IHttpClientFactory>();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            _service = new WorkflowManager(_db, mockClientFactory.Object, mockHttpContextAccessor.Object);
        }

        [TearDown]
        public void TearDown() => _db.Dispose();

        [Test]
        public async Task SavePricing_Valid_ReturnsTrue() // TC07
        {
            var result = await _service.SavePricingAsync(1, new PricingDto { MRP = 100, SalePrice = 90 });
            Assert.That(result, Is.True);
            var saved = await _db.Pricings.FindAsync(1);
            Assert.That(saved!.SalePrice, Is.EqualTo(90));
        }

        [Test]
        public void SavePricing_InvalidRule_ThrowsException() // TC08
        {
            Assert.ThrowsAsync<ArgumentException>(() =>
                _service.SavePricingAsync(1, new PricingDto { MRP = 100, SalePrice = 110 }));
        }

        [Test]
        public async Task SubmitForReview_Valid_UpdatesStatus() // TC09
        {
            await _service.SubmitForReviewAsync(1, "pm@test.com");
            var approval = await _db.Approvals.FirstOrDefaultAsync(a => a.ProductId == 1);
            Assert.That(approval!.Status, Is.EqualTo("Ready for Review"));
        }

        [Test]
        public async Task UpdateStatus_AdminApprove_UpdatesStatus() // TC10
        {
            await _service.UpdateStatusAsync(1, new StatusUpdateDto { Status = "Approved" }, "admin@test.com");
            var approval = await _db.Approvals.FirstOrDefaultAsync(a => a.ProductId == 1);
            Assert.That(approval!.Status, Is.EqualTo("Approved"));
            Assert.That(approval!.ApprovedBy, Is.EqualTo("admin@test.com"));
        }
    }
}
