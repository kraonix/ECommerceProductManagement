using System.ComponentModel.DataAnnotations;

namespace CatalogService.DTOs
{
    public class ProductCreateDto
    {
        public int CategoryId { get; set; }

        [Required, MaxLength(50)] 
        public string SKU { get; set; } = string.Empty;

        [Required, MaxLength(200)] 
        public string Name { get; set; } = string.Empty;

        public string Brand { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; } = 99.99m;
        public int StockQuantity { get; set; } = 0;
        public decimal WeightKg { get; set; } = 0.0m;
        public string DimensionsCm { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string WarrantyPeriod { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string Highlights { get; set; } = string.Empty;
        public string HardwareInterface { get; set; } = string.Empty;
    }

    public class ProductUpdateDto : ProductCreateDto { }

    public class ProductResponseDto
    {
        public int ProductId { get; set; }
        public int CategoryId { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PublishStatus { get; set; } = string.Empty;
        
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public decimal WeightKg { get; set; }
        public string DimensionsCm { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string WarrantyPeriod { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string Highlights { get; set; } = string.Empty;
        public string HardwareInterface { get; set; } = string.Empty;
    }

    public class MediaUploadDto
    {
        [Required] public string FileName { get; set; } = string.Empty;
        [Required] public string Base64Content { get; set; } = string.Empty;
    }
}
