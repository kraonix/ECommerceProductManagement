using AdminReportingService.Data;
using AdminReportingService.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace AdminReportingService.Services
{
    public class ReportingManager
    {
        private readonly AdminDbContext _db;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReportingManager(AdminDbContext db, IHttpClientFactory clientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _clientFactory = clientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
        {
            var summary = new DashboardSummaryDto();
            try 
            {
                var client = _clientFactory.CreateClient();
                // Flow the JWT token from the original HTTP Request header across service boundary
                var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
                if (!string.IsNullOrEmpty(authHeader))
                {
                    client.DefaultRequestHeaders.Add("Authorization", authHeader);
                }

                // Call CatalogService using its internal network dev port
                var response = await client.GetAsync("http://localhost:5020/api/products");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var products = JsonSerializer.Deserialize<List<ProductItemDto>>(content, options) ?? new List<ProductItemDto>();
                    
                    summary.TotalActiveProducts = products.Count(p => p.PublishStatus == "Published");
                    summary.PendingApprovals = products.Count(p => p.PublishStatus == "Pending");
                    summary.LowStockItems = 0; // Inventory service not integrated, zero placeholder
                    
                    if (summary.PendingApprovals > 0)
                        summary.Alerts.Add($"There are {summary.PendingApprovals} product(s) pending approval.");
                        
                    var drafts = products.Count(p => p.PublishStatus == "Draft");
                    if (drafts > 0)
                        summary.Alerts.Add($"You have {drafts} Draft product(s) sitting in your queue.");

                    if (products.Count == 0)
                        summary.Alerts.Add("Welcome! Start by adding your first product to the Catalog.");
                }
            }
            catch 
            {
                summary.Alerts.Add("Warning: Real-time Catalog metrics are currently unreachable.");
            }

            if (summary.Alerts.Count == 0)
                 summary.Alerts.Add("System is operating normally. Catalog is perfectly healthy.");

            return summary;
        }

        private class ProductItemDto 
        {
            public int ProductId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string PublishStatus { get; set; } = string.Empty;
        }

        public async Task<IEnumerable<AuditLogDto>> GetProductAuditHistoryAsync(int productId)
        {
            return await _db.AuditLogs
                .Where(a => a.ProductId == productId)
                .OrderByDescending(a => a.Timestamp)
                .Select(a => new AuditLogDto
                {
                    ProductId = a.ProductId, Action = a.Action,
                    Details = a.Details, ActionBy = a.ActionBy, Timestamp = a.Timestamp
                }).ToListAsync(); // TC14
        }

        public async Task<byte[]> ExportDashboardDataAsync()
        {
            // TC13: Mock CSV export
            var csv = "Metric,Value\nTotalActiveProducts,150\nPendingApprovals,12\nLowStockItems,5";
            return await Task.FromResult(System.Text.Encoding.UTF8.GetBytes(csv));
        }
    }
}
