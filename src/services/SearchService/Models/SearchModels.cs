using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SearchService.Models
{
    public class SearchRequest
    {
        [MaxLength(200)]
        public string? Query { get; set; }

        [MaxLength(100)]
        public string? Brand { get; set; }

        [Range(0, 1_000_000)]
        public decimal? MinPrice { get; set; }

        [Range(0, 1_000_000)]
        public decimal? MaxPrice { get; set; }

        public bool InStockOnly { get; set; } = false;

        // Allowed values enforced in SearchEngine
        [MaxLength(20)]
        public string SortBy { get; set; } = "relevance";

        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        // Controller already clamps this to 1–100 via Math.Clamp
        [Range(1, 100)]
        public int PageSize { get; set; } = 20;
    }

    public class SearchResult
    {
        public int ProductId { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PublishStatus { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Material { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string WarrantyPeriod { get; set; } = string.Empty;
        public string Highlights { get; set; } = string.Empty;
        public string HardwareInterface { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public List<string> Photos { get; set; } = new();

        // Relevance score (not exposed to client but used for sorting)
        public double RelevanceScore { get; set; }

        // Matched fields for highlighting
        public List<string> MatchedFields { get; set; } = new();
    }

    public class SearchFacets
    {
        public List<BrandFacet> Brands { get; set; } = new();
        public PriceRangeFacet PriceRange { get; set; } = new();
        public int TotalInStock { get; set; }
    }

    public class BrandFacet
    {
        public string Brand { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class PriceRangeFacet
    {
        public decimal Min { get; set; }
        public decimal Max { get; set; }
    }

    public class SearchResponse
    {
        public string Query { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public List<SearchResult> Results { get; set; } = new();
        public SearchFacets Facets { get; set; } = new();
    }

    public class SuggestResponse
    {
        public string Query { get; set; } = string.Empty;
        public List<SuggestionItem> Suggestions { get; set; } = new();
    }

    public class SuggestionItem
    {
        public string Text { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // product | brand | category
        public int? ProductId { get; set; }
    }
}
