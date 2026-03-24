using System;
using System.ComponentModel.DataAnnotations;

namespace ProductWorkflowService.Entities
{
    public class ProductPricing
    {
        [Key] public int ProductId { get; set; }
        [Range(0.01, 100000)] public decimal MRP { get; set; }
        [Range(0.01, 100000)] public decimal SalePrice { get; set; }
        public decimal GST { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ProductInventory
    {
        [Key] public int ProductId { get; set; }
        public int AvailableQty { get; set; }
        public int ReservedQty { get; set; }
        public string WarehouseLocation { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ProductApproval
    {
        [Key] public int ApprovalId { get; set; }
        public int ProductId { get; set; }
        [Required, MaxLength(50)] public string Status { get; set; } = "Draft";
        public string Remarks { get; set; } = string.Empty;
        public string SubmittedBy { get; set; } = string.Empty;
        public string ApprovedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
