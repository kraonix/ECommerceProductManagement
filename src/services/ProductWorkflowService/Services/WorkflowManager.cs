using Microsoft.EntityFrameworkCore;
using ProductWorkflowService.Data;
using ProductWorkflowService.DTOs;
using ProductWorkflowService.Entities;
using System;
using System.Threading.Tasks;

namespace ProductWorkflowService.Services
{
    public class WorkflowManager
    {
        private readonly WorkflowDbContext _db;

        public WorkflowManager(WorkflowDbContext db) => _db = db;

        public async Task<bool> SavePricingAsync(int productId, PricingDto dto)
        {
            if (dto.SalePrice > dto.MRP)
                throw new ArgumentException("Sale price cannot be higher than MRP."); // TC08

            var pricing = await _db.Pricings.FindAsync(productId);
            if (pricing == null)
            {
                pricing = new ProductPricing { ProductId = productId };
                _db.Pricings.Add(pricing);
            }

            pricing.MRP = dto.MRP;
            pricing.SalePrice = dto.SalePrice;
            pricing.GST = dto.GST;
            pricing.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(); // TC07
            return true;
        }

        public async Task<bool> SaveInventoryAsync(int productId, InventoryDto dto)
        {
            var inv = await _db.Inventories.FindAsync(productId);
            if (inv == null)
            {
                inv = new ProductInventory { ProductId = productId };
                _db.Inventories.Add(inv);
            }

            inv.AvailableQty = dto.AvailableQty;
            inv.WarehouseLocation = dto.WarehouseLocation;
            inv.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SubmitForReviewAsync(int productId, string userEmail)
        {
            var approval = new ProductApproval
            {
                ProductId = productId,
                Status = "Ready for Review", // TC09
                SubmittedBy = userEmail,
                CreatedAt = DateTime.UtcNow
            };

            _db.Approvals.Add(approval);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateStatusAsync(int productId, StatusUpdateDto dto, string adminEmail)
        {
            var approval = await _db.Approvals.FirstOrDefaultAsync(a => a.ProductId == productId);
            if (approval == null)
            {
                approval = new ProductApproval { ProductId = productId };
                _db.Approvals.Add(approval);
            }

            if ((dto.Status == "Rejected" || dto.Status == "Archived") && string.IsNullOrWhiteSpace(dto.Remarks))
                throw new ArgumentException("Remarks are required for Rejection or Archive.");

            approval.Status = dto.Status;
            approval.Remarks = dto.Remarks;
            approval.ApprovedBy = adminEmail;
            approval.CreatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(); // TC10, TC11
            return true;
        }
    }
}
