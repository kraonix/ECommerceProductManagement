using System;

namespace AdminReportingService.DTOs
{
    public class DashboardSummaryDto
    {
        public int TotalActiveProducts { get; set; }
        public int PendingApprovals { get; set; }
        public int LowStockItems { get; set; }
        public System.Collections.Generic.List<string> Alerts { get; set; } = new System.Collections.Generic.List<string>();
    }

    public class AuditLogDto
    {
        public int ProductId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string ActionBy { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
