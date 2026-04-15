using Microsoft.EntityFrameworkCore;
using SearchService.Data;
using SearchService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SearchService.Services
{
    public class SearchEngine
    {
        private readonly SearchDbContext _db;

        // Field weights for relevance scoring
        private const double W_SKU_EXACT       = 120.0;
        private const double W_NAME_EXACT      = 100.0;
        private const double W_NAME_STARTS     = 85.0;
        private const double W_NAME_WORD       = 70.0;
        private const double W_NAME_CONTAINS   = 55.0;
        private const double W_BRAND_EXACT     = 60.0;
        private const double W_BRAND_CONTAINS  = 40.0;
        private const double W_HIGHLIGHTS      = 30.0;
        private const double W_MATERIAL        = 25.0;
        private const double W_COLOR           = 20.0;
        private const double W_DESCRIPTION     = 15.0;
        private const double W_FUZZY           = 10.0;

        public SearchEngine(SearchDbContext db) => _db = db;

        public async Task<SearchResponse> SearchAsync(SearchRequest req)
        {
            // Load all published, non-archived products with media
            var allProducts = await _db.Products
                .Include(p => p.MediaAssets)
                .Where(p => p.PublishStatus == "Published" && !p.IsArchived)
                .ToListAsync();

            var query = (req.Query ?? "").Trim().ToLowerInvariant();
            var queryTokens = TokenizeQuery(query);

            // Score every product
            var scored = allProducts
                .Select(p => ScoreProduct(p, query, queryTokens))
                .ToList();

            // Filter: only include products with meaningful score when query is provided
            if (!string.IsNullOrEmpty(query))
                scored = scored.Where(r => r.RelevanceScore >= 15).ToList();

            // Apply filters
            if (!string.IsNullOrEmpty(req.Brand))
                scored = scored.Where(r => r.Brand.Equals(req.Brand, StringComparison.OrdinalIgnoreCase)).ToList();

            if (req.MinPrice.HasValue)
                scored = scored.Where(r => r.Price >= req.MinPrice.Value).ToList();

            if (req.MaxPrice.HasValue)
                scored = scored.Where(r => r.Price <= req.MaxPrice.Value).ToList();

            if (req.InStockOnly)
                scored = scored.Where(r => r.StockQuantity > 0).ToList();

            // Build facets from filtered (pre-sort) results
            var facets = BuildFacets(scored, allProducts.Select(p => new SearchResult
            {
                Brand = p.Brand,
                Price = p.Price,
                StockQuantity = p.StockQuantity
            }).ToList());

            // Sort
            scored = req.SortBy switch
            {
                "price-asc"  => scored.OrderBy(r => r.Price).ToList(),
                "price-desc" => scored.OrderByDescending(r => r.Price).ToList(),
                "name-asc"   => scored.OrderBy(r => r.Name).ToList(),
                "newest"     => scored.OrderByDescending(r => r.ProductId).ToList(),
                _            => scored.OrderByDescending(r => r.RelevanceScore).ThenBy(r => r.Name).ToList()
            };

            var totalCount = scored.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)req.PageSize);
            var page = Math.Max(1, req.Page);
            var paged = scored.Skip((page - 1) * req.PageSize).Take(req.PageSize).ToList();

            return new SearchResponse
            {
                Query = req.Query ?? "",
                TotalCount = totalCount,
                Page = page,
                PageSize = req.PageSize,
                TotalPages = totalPages,
                Results = paged,
                Facets = facets
            };
        }

        public async Task<SuggestResponse> SuggestAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return new SuggestResponse { Query = query };

            var q = query.Trim().ToLowerInvariant();

            var products = await _db.Products
                .Where(p => p.PublishStatus == "Published" && !p.IsArchived)
                .ToListAsync();

            var suggestions = new List<SuggestionItem>();

            // Product name suggestions
            var nameMatches = products
                .Where(p => p.Name.ToLowerInvariant().Contains(q))
                .OrderBy(p => p.Name.ToLowerInvariant().IndexOf(q))
                .Take(5)
                .Select(p => new SuggestionItem
                {
                    Text = p.Name,
                    Type = "product",
                    ProductId = p.ProductId
                });
            suggestions.AddRange(nameMatches);

            // Brand suggestions
            var brandMatches = products
                .Where(p => p.Brand.ToLowerInvariant().Contains(q))
                .Select(p => p.Brand)
                .Distinct()
                .Take(3)
                .Select(b => new SuggestionItem { Text = b, Type = "brand" });
            suggestions.AddRange(brandMatches);

            // SKU suggestions
            var skuMatches = products
                .Where(p => p.SKU.ToLowerInvariant().Contains(q))
                .Take(3)
                .Select(p => new SuggestionItem
                {
                    Text = p.SKU,
                    Type = "sku",
                    ProductId = p.ProductId
                });
            suggestions.AddRange(skuMatches);

            // Deduplicate and limit
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var deduped = suggestions
                .Where(s => seen.Add(s.Text))
                .Take(8)
                .ToList();

            return new SuggestResponse { Query = query, Suggestions = deduped };
        }

        // ── Core scoring ──────────────────────────────────────────────────────

        private SearchResult ScoreProduct(Entities.ProductIndex p, string query, string[] tokens)
        {
            var result = new SearchResult
            {
                ProductId = p.ProductId,
                SKU = p.SKU,
                Name = p.Name,
                Brand = p.Brand,
                Description = p.Description,
                PublishStatus = p.PublishStatus,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                Material = p.Material,
                Color = p.Color,
                WarrantyPeriod = p.WarrantyPeriod,
                Highlights = p.Highlights,
                HardwareInterface = p.HardwareInterface,
                Manufacturer = p.Manufacturer,
                Photos = p.MediaAssets.Select(m => m.Url).ToList()
            };

            if (string.IsNullOrEmpty(query))
            {
                result.RelevanceScore = 1.0; // no query = show everything equally
                return result;
            }

            double score = 0;
            var matched = new List<string>();

            var nameLower  = p.Name.ToLowerInvariant();
            var brandLower = p.Brand.ToLowerInvariant();
            var skuLower   = p.SKU.ToLowerInvariant();
            var descLower  = p.Description.ToLowerInvariant();
            var hlLower    = p.Highlights.ToLowerInvariant();
            var matLower   = p.Material.ToLowerInvariant();
            var colorLower = p.Color.ToLowerInvariant();

            // ── Exact / prefix / contains on full query ──
            if (skuLower == query)                    { score += W_SKU_EXACT;   matched.Add("sku"); }
            else if (skuLower.Contains(query))        { score += W_SKU_EXACT * 0.7; matched.Add("sku"); }

            if (nameLower == query)                   { score += W_NAME_EXACT;  matched.Add("name"); }
            else if (nameLower.StartsWith(query))     { score += W_NAME_STARTS; matched.Add("name"); }
            else if (nameLower.Contains(query))       { score += W_NAME_CONTAINS; matched.Add("name"); }

            if (brandLower == query)                  { score += W_BRAND_EXACT;    matched.Add("brand"); }
            else if (brandLower.Contains(query))      { score += W_BRAND_CONTAINS; matched.Add("brand"); }

            if (hlLower.Contains(query))              { score += W_HIGHLIGHTS;  matched.Add("highlights"); }
            if (matLower.Contains(query))             { score += W_MATERIAL;    matched.Add("material"); }
            if (colorLower.Contains(query))           { score += W_COLOR;       matched.Add("color"); }
            if (descLower.Contains(query))            { score += W_DESCRIPTION; matched.Add("description"); }

            // ── Token-level scoring (multi-word queries) ──
            foreach (var token in tokens)
            {
                if (token.Length < 2) continue;

                if (nameLower.Contains(token))        score += W_NAME_WORD * 0.8;
                if (brandLower.Contains(token))       score += W_BRAND_CONTAINS * 0.8;
                if (hlLower.Contains(token))          score += W_HIGHLIGHTS * 0.7;
                if (descLower.Contains(token))        score += W_DESCRIPTION * 0.7;
                if (matLower.Contains(token))         score += W_MATERIAL * 0.7;
                if (colorLower.Contains(token))       score += W_COLOR * 0.7;
            }

            // ── Fuzzy matching (typo tolerance) ──
            // Only run fuzzy if no exact/contains match found yet
            // Fuzzy ONLY on product name words — never on description/highlights (too noisy)
            if (score == 0 && query.Length >= 4)
            {
                var nameWords = nameLower.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach (var word in nameWords)
                {
                    // Skip very short words — too many false positives
                    if (word.Length < 4) continue;

                    // Length ratio guard: words must be within 30% length of each other
                    var lengthRatio = (double)Math.Abs(query.Length - word.Length) / Math.Max(query.Length, word.Length);
                    if (lengthRatio > 0.3) continue;

                    var dist = LevenshteinDistance(query, word);

                    // Strict distance: max 1 for short queries (4-5 chars), max 2 for longer (6+)
                    // AND distance must be < 40% of query length to avoid "table"→"cable" type matches
                    var maxDist = query.Length <= 5 ? 1 : 2;
                    var distRatio = (double)dist / query.Length;

                    if (dist <= maxDist && distRatio < 0.35)
                    {
                        score += W_FUZZY * (1.0 - distRatio);
                        matched.Add("fuzzy:name");
                        break;
                    }
                }

                // Fuzzy on brand — only exact brand word match with distance 1
                if (score == 0 && query.Length >= 5)
                {
                    var brandWords = brandLower.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var word in brandWords)
                    {
                        if (word.Length < 4) continue;
                        var lengthRatio = (double)Math.Abs(query.Length - word.Length) / Math.Max(query.Length, word.Length);
                        if (lengthRatio > 0.25) continue;
                        var dist = LevenshteinDistance(query, word);
                        if (dist == 1)
                        {
                            score += W_FUZZY * 0.5;
                            matched.Add("fuzzy:brand");
                            break;
                        }
                    }
                }
            }

            // ── Boost in-stock products slightly ──
            if (p.StockQuantity > 0) score *= 1.05;

            // ── Minimum score threshold ──
            // Pure fuzzy-only matches (score ≤ 12) are too weak — suppress them
            // This prevents garbage results like "table" → NVMe SSD
            if (score > 0 && score <= 12 && !matched.Any(m => !m.StartsWith("fuzzy")))
                score = 0;

            result.RelevanceScore = Math.Round(score, 2);
            result.MatchedFields = matched.Distinct().ToList();
            return result;
        }

        // ── Facets ────────────────────────────────────────────────────────────

        private static SearchFacets BuildFacets(List<SearchResult> filtered, List<SearchResult> all)
        {
            var facets = new SearchFacets();

            // Brand facets from filtered results
            facets.Brands = filtered
                .Where(r => !string.IsNullOrEmpty(r.Brand))
                .GroupBy(r => r.Brand, StringComparer.OrdinalIgnoreCase)
                .Select(g => new BrandFacet { Brand = g.Key, Count = g.Count() })
                .OrderByDescending(b => b.Count)
                .ToList();

            // Price range from ALL products (not filtered)
            if (all.Any())
            {
                facets.PriceRange = new PriceRangeFacet
                {
                    Min = all.Min(r => r.Price),
                    Max = all.Max(r => r.Price)
                };
            }

            facets.TotalInStock = filtered.Count(r => r.StockQuantity > 0);
            return facets;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static string[] TokenizeQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return Array.Empty<string>();
            return query
                .Split(new[] { ' ', '-', '/', '+', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(t => t.Length >= 2)
                .ToArray();
        }

        /// <summary>
        /// Levenshtein edit distance — used for fuzzy/typo-tolerant matching.
        /// </summary>
        private static int LevenshteinDistance(string s, string t)
        {
            if (string.IsNullOrEmpty(s)) return t?.Length ?? 0;
            if (string.IsNullOrEmpty(t)) return s.Length;

            var m = s.Length;
            var n = t.Length;
            var d = new int[m + 1, n + 1];

            for (var i = 0; i <= m; i++) d[i, 0] = i;
            for (var j = 0; j <= n; j++) d[0, j] = j;

            for (var j = 1; j <= n; j++)
            for (var i = 1; i <= m; i++)
            {
                var cost = s[i - 1] == t[j - 1] ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }

            return d[m, n];
        }
    }
}
