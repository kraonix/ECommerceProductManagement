using CatalogService.DTOs;
using CatalogService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogService.Controllers
{
    [ApiController]
    [Route("api/products")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly ProductService _productService;
        
        public ProductsController(ProductService productService) => _productService = productService;    
        
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? search) => Ok(await _productService.GetProductsAsync(search));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            return product == null ? NotFound() : Ok(product);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,ProductManager")]
        public async Task<IActionResult> Create(ProductCreateDto dto)
        {
            try { return Created("", await _productService.CreateProductAsync(dto)); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,ProductManager")]
        public async Task<IActionResult> Update(int id, ProductUpdateDto dto)
        {
            try 
            {
                var success = await _productService.UpdateProductAsync(id, dto);
                return success ? NoContent() : NotFound();
            }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        }

        [HttpPost("{id}/media")]
        [Authorize(Roles = "Admin,ProductManager,ContentExecutive")]
        public async Task<IActionResult> UploadMedia(int id, MediaUploadDto dto)
        {
            try 
            {
                var success = await _productService.UploadMediaAsync(id, dto);
                return success ? Ok(new { message = "Media uploaded successfully." }) : NotFound(new { message = "Product not found." });
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpDelete("{id}/media/{mediaId}")]
        [Authorize(Roles = "Admin,ProductManager,ContentExecutive")]
        public async Task<IActionResult> DeleteMedia(int id, int mediaId)
        {
            var success = await _productService.DeleteMediaAsync(id, mediaId);
            return success ? Ok(new { message = "Image deleted." }) : NotFound(new { message = "Media record not found." });
        }

        [HttpDelete("{id}/media")]
        [Authorize(Roles = "Admin,ProductManager")]
        public async Task<IActionResult> DeleteAllMedia(int id)
        {
            var count = await _productService.DeleteAllMediaAsync(id);
            return Ok(new { message = $"{count} image(s) deleted." });
        }

        /// <summary>
        /// Diagnostic endpoint — checks DB media records and file system for a product.
        /// GET /api/products/{id}/media/debug
        /// </summary>
        [HttpGet("{id}/media/debug")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DebugMedia(int id,
            [Microsoft.AspNetCore.Mvc.FromServices] CatalogService.Data.CatalogDbContext db)
        {
            var mediaRecords = await db.MediaAssets
                .Where(m => m.ProductId == id)
                .Select(m => new { m.MediaId, m.Url, m.AssetType, m.IsMain })
                .ToListAsync();

            var uploadsDir = Path.GetFullPath(
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "wwwroot", "uploads"));

            var filesOnDisk = Directory.Exists(uploadsDir)
                ? Directory.GetFiles(uploadsDir).Select(f => Path.GetFileName(f)).ToList()
                : new System.Collections.Generic.List<string>();

            var fileChecks = mediaRecords.Select(m =>
            {
                var fileName = Path.GetFileName(m.Url);
                var fullPath = Path.Combine(uploadsDir, fileName);
                return new
                {
                    m.MediaId,
                    m.Url,
                    FileExistsOnDisk = System.IO.File.Exists(fullPath),
                    FullPath = fullPath
                };
            }).ToList();

            return Ok(new
            {
                ProductId = id,
                DbRecordCount = mediaRecords.Count,
                DbRecords = mediaRecords,
                UploadsDirectory = uploadsDir,
                DirectoryExists = Directory.Exists(uploadsDir),
                FilesOnDisk = filesOnDisk.Count,
                FileChecks = fileChecks,
                UploadLogPath = Path.GetFullPath(
                    Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "logs", "upload_logs.txt"))
            });
        }
    }
}
