using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatalogService.DTOs
{
    public class ProductCreateDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "CategoryId must be a positive integer.")]
        public int CategoryId { get; set; }

        [Required, MaxLength(50)]
        public string SKU { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Brand { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Range(0.01, 1_000_000, ErrorMessage = "Price must be between 0.01 and 1,000,000.")]
        public decimal Price { get; set; } = 99.99m;

        [Range(0, 1_000_000, ErrorMessage = "StockQuantity must be between 0 and 1,000,000.")]
        public int StockQuantity { get; set; } = 0;

        [Range(0, 10_000, ErrorMessage = "WeightKg must be between 0 and 10,000.")]
        public decimal WeightKg { get; set; } = 0.0m;

        [MaxLength(50)]
        public string DimensionsCm { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Material { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Color { get; set; } = string.Empty;

        [MaxLength(50)]
        public string WarrantyPeriod { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Manufacturer { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Highlights { get; set; } = string.Empty;

        [MaxLength(100)]
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

        // Media assets — list of image URLs
        public List<string> Photos { get; set; } = new List<string>();

        // Media assets with IDs for deletion
        public List<MediaAssetDto> MediaAssets { get; set; } = new List<MediaAssetDto>();

        // Archive info
        public bool IsArchived { get; set; }
        public DateTime? ArchivedAt { get; set; }
        public string? ArchivedBy { get; set; }
        public string? ArchivedReason { get; set; }
    }

    public class MediaAssetDto
    {
        public int MediaId { get; set; }
        public string Url { get; set; } = string.Empty;
    }

    public class MediaUploadDto
    {
        [Required, MaxLength(260)]
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Base64-encoded image content. Max ~10 MB decoded (≈ 13.6 MB base64).
        /// </summary>
        [Required, MaxLength(14_000_000)]
        public string Base64Content { get; set; } = string.Empty;
    }

    public class ArchiveProductDto
    {
        [Required, MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }
}
