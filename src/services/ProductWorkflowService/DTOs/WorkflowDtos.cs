using System.ComponentModel.DataAnnotations;

namespace ProductWorkflowService.DTOs
{
    public class PricingDto
    {
        [Required]
        [Range(0.01, 1_000_000, ErrorMessage = "MRP must be between 0.01 and 1,000,000.")]
        public decimal MRP { get; set; }

        [Required]
        [Range(0.01, 1_000_000, ErrorMessage = "SalePrice must be between 0.01 and 1,000,000.")]
        public decimal SalePrice { get; set; }

        [Range(0, 100, ErrorMessage = "GST must be between 0 and 100.")]
        public decimal GST { get; set; }
    }

    public class InventoryDto
    {
        [Required]
        [Range(0, 1_000_000, ErrorMessage = "AvailableQty must be between 0 and 1,000,000.")]
        public int AvailableQty { get; set; }

        [MaxLength(100)]
        public string WarehouseLocation { get; set; } = string.Empty;
    }

    public class StatusUpdateDto
    {
        private static readonly string[] AllowedStatuses =
        [
            "Draft", "In Enrichment", "Ready for Review",
            "Approved", "Rejected", "Published", "Archived"
        ];

        [Required]
        public string Status { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Remarks { get; set; } = string.Empty;

        public bool IsValidStatus() => AllowedStatuses.Contains(Status);
    }
}
