using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SearchService.Entities
{
    /// <summary>
    /// Read-only projection of the CatalogDB Products table used for search indexing.
    /// </summary>
    public class ProductIndex
    {
        [Key]
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
        public bool IsArchived { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<MediaAssetIndex> MediaAssets { get; set; } = new List<MediaAssetIndex>();
    }

    public class MediaAssetIndex
    {
        [Key]
        public int MediaId { get; set; }
        public int ProductId { get; set; }
        public string Url { get; set; } = string.Empty;
        public string AssetType { get; set; } = string.Empty;
    }
}
