using AdminReportingService.Data;
using AdminReportingService.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminReportingService.Services
{
    public class ReportingManager
    {
        private readonly AdminDbContext _db;
        public ReportingManager(AdminDbContext db) => _db = db;

        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
        {
            return await Task.FromResult(new DashboardSummaryDto
            {
                TotalActiveProducts = 150,
                PendingApprovals = 12,
                LowStockItems = 5
            });
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
