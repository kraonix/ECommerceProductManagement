using System.ComponentModel.DataAnnotations;

namespace ProductWorkflowService.DTOs
{
    public class PricingDto
    {
        [Required] public decimal MRP { get; set; }
        [Required] public decimal SalePrice { get; set; }
        public decimal GST { get; set; }
    }

    public class InventoryDto
    {
        [Required] public int AvailableQty { get; set; }
        public string WarehouseLocation { get; set; } = string.Empty;
    }

    public class StatusUpdateDto
    {
        [Required] public string Status { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
    }
}
