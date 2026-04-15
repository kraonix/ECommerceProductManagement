using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ProductWorkflowService.Data;
using ProductWorkflowService.DTOs;
using ProductWorkflowService.Entities;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ProductWorkflowService.Services
{
    public class WorkflowManager
    {
        private readonly WorkflowDbContext _db;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WorkflowManager(WorkflowDbContext db, IHttpClientFactory clientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _clientFactory = clientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

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

            var actor = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "system";
            await WriteAuditAsync(productId, "PricingUpdated", $"MRP={dto.MRP}, SalePrice={dto.SalePrice}, GST={dto.GST}", actor);

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

            var actor = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "system";
            await WriteAuditAsync(productId, "InventoryUpdated", $"Qty={dto.AvailableQty}, Location={dto.WarehouseLocation}", actor);

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

            await WriteAuditAsync(productId, "SubmittedForReview", "Product submitted for admin approval.", userEmail);

            return true;
        }

        public async Task<bool> MarkInEnrichmentAsync(int productId, string userEmail)
        {
            var approval = await _db.Approvals.FirstOrDefaultAsync(a => a.ProductId == productId);
            if (approval == null)
            {
                approval = new ProductApproval { ProductId = productId };
                _db.Approvals.Add(approval);
            }

            if (approval.Status != "Draft" && approval.Status != "In Enrichment")
                throw new ArgumentException($"Cannot move to 'In Enrichment' from '{approval.Status}'. Only Draft products can be enriched.");

            approval.Status = "In Enrichment";
            approval.SubmittedBy = userEmail;
            approval.CreatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            await WriteAuditAsync(productId, "InEnrichment", "Product moved to In Enrichment stage.", userEmail);

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

            var validStatuses = new[] { "Draft", "In Enrichment", "Ready for Review", "Approved", "Published", "Rejected", "Archived" };
            if (!validStatuses.Contains(dto.Status))
                throw new ArgumentException($"Invalid status '{dto.Status}'. Valid values: {string.Join(", ", validStatuses)}");

            approval.Status = dto.Status;
            approval.Remarks = dto.Remarks;
            approval.ApprovedBy = adminEmail;
            approval.CreatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(); // TC10, TC11

            var details = string.IsNullOrWhiteSpace(dto.Remarks)
                ? $"Status changed to {dto.Status}."
                : $"Status changed to {dto.Status}. Remarks: {dto.Remarks}";
            await WriteAuditAsync(productId, $"StatusChanged:{dto.Status}", details, adminEmail);

            return true;
        }

        /// <summary>
        /// Fire-and-forget audit log write to AdminReportingService.
        /// Failures are swallowed so they never break the main workflow.
        /// </summary>
        private async Task WriteAuditAsync(int productId, string action, string details, string actionBy)
        {
            try
            {
                var client = _clientFactory.CreateClient();

                // Forward the JWT so AdminReportingService can authorize the request
                var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
                if (!string.IsNullOrEmpty(authHeader))
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authHeader);

                var payload = new
                {
                    ProductId = productId,
                    Action = action,
                    Details = details,
                    ActionBy = actionBy
                };

                await client.PostAsJsonAsync("http://localhost:5040/api/audit/log", payload);
            }
            catch
            {
                // Audit failures must never break the main operation
            }
        }
    }
}
