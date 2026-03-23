using System.ComponentModel.DataAnnotations;

namespace CatalogService.Entities
{
    public class ProductVariant
    {
        [Key]
        public int VariantId { get; set; }
        public int ProductId { get; set; }

        [MaxLength(50)] 
        public string Color { get; set; } = string.Empty;

        [MaxLength(50)] 
        public string Size { get; set; } = string.Empty;

        [MaxLength(100)] 
        public string Barcode { get; set; } = string.Empty;

        public Product Product { get; set; } = null!;
    }
}
