using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatalogService.Entities
{
    public class Product
    {
        public int ProductId { get; set; }
        public int CategoryId { get; set; }

        [Required, MaxLength(50)] 
        public string SKU { get; set; } = string.Empty;

        [Required, MaxLength(200)] 
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)] 
        public string Brand { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required, MaxLength(50)] 
        public string PublishStatus { get; set; } = "Draft";

        // New Realism Data Fields
        public decimal Price { get; set; } = 99.99m;
        public int StockQuantity { get; set; } = 0;
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

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Category Category { get; set; } = null!;
        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public ICollection<MediaAsset> MediaAssets { get; set; } = new List<MediaAsset>();
    }
}
