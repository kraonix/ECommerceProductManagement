using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SearchService.Models;
using SearchService.Services;
using System.Threading.Tasks;

namespace SearchService.Controllers
{
    [ApiController]
    [Route("api/search")]
    [Authorize]
    public class SearchController : ControllerBase
    {
        private readonly SearchEngine _engine;

        public SearchController(SearchEngine engine) => _engine = engine;

        /// <summary>
        /// Full search with relevance scoring, filters, facets and pagination.
        /// GET /api/search?q=keyboard&brand=Obsidian&minPrice=50&maxPrice=500&inStockOnly=true&sortBy=relevance&page=1&pageSize=20
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Search(
            [FromQuery] string? q,
            [FromQuery] string? brand,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] bool inStockOnly = false,
            [FromQuery] string sortBy = "relevance",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var req = new SearchRequest
            {
                Query = q,
                Brand = brand,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                InStockOnly = inStockOnly,
                SortBy = sortBy,
                Page = page,
                PageSize = Math.Clamp(pageSize, 1, 100)
            };

            var result = await _engine.SearchAsync(req);
            return Ok(result);
        }

        /// <summary>
        /// Autocomplete suggestions as the user types.
        /// GET /api/search/suggest?q=key
        /// </summary>
        [HttpGet("suggest")]
        public async Task<IActionResult> Suggest([FromQuery] string? q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
                return Ok(new SuggestResponse { Query = q ?? "" });

            var result = await _engine.SuggestAsync(q);
            return Ok(result);
        }

        /// <summary>
        /// Health check.
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult Health() => Ok(new { status = "healthy", service = "SearchService" });
    }
}
