using System;
using System.ComponentModel.DataAnnotations;

namespace AdminReportingService.Entities
{
    public class AuditLog
    {
        [Key] public int AuditId { get; set; }
        public int ProductId { get; set; }
        [MaxLength(100)] public string EntityName { get; set; } = string.Empty;
        [MaxLength(50)] public string Action { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string ActionBy { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
