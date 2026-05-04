using System;
using System.ComponentModel.DataAnnotations;

namespace AdminReportingService.DTOs
{
    public class DashboardSummaryDto
    {
        [Range(0, int.MaxValue)]
        public int TotalActiveProducts { get; set; }

        [Range(0, int.MaxValue)]
        public int PendingApprovals { get; set; }

        [Range(0, int.MaxValue)]
        public int LowStockItems { get; set; }

        public System.Collections.Generic.List<string> Alerts { get; set; } = new System.Collections.Generic.List<string>();
    }

    public class AuditLogDto
    {
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }

        [Required, MaxLength(50)]
        public string Action { get; set; } = string.Empty;

        [Required, MaxLength(2000)]
        public string Details { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string ActionBy { get; set; } = string.Empty;

        [Required]
        public DateTime Timestamp { get; set; }
    }
}
