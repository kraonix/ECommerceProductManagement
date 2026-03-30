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
    }

    public class MediaUploadDto
    {
        [Required] public string FileName { get; set; } = string.Empty;
        [Required] public string Base64Content { get; set; } = string.Empty;
    }
}
