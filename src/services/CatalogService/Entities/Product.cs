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

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Category Category { get; set; } = null!;
        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public ICollection<MediaAsset> MediaAssets { get; set; } = new List<MediaAsset>();
    }
}
