using System.ComponentModel.DataAnnotations;

namespace CatalogService.Entities
{
    public class MediaAsset
    {
        [Key]
        public int MediaId { get; set; }
        public int ProductId { get; set; }

        [Required, MaxLength(500)] 
        public string Url { get; set; } = string.Empty;

        [Required, MaxLength(20)] 
        public string AssetType { get; set; } = "Image";

        public bool IsMain { get; set; } = false;

        public Product Product { get; set; } = null!;
    }
}
