using CatalogService.Data;
using CatalogService.DTOs;
using CatalogService.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogService.Services
{
    public class ProductService
    {
        private readonly CatalogDbContext _db;
        private readonly string _uploadsDir;
        private static readonly SemaphoreSlim _logLock = new SemaphoreSlim(1, 1);

        private static readonly string _uploadLogPath = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "logs", "upload_logs.txt"));

        public ProductService(CatalogDbContext db, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        {
            _db = db;
            _uploadsDir = Path.Combine(env.ContentRootPath, "wwwroot", "uploads");
            Directory.CreateDirectory(_uploadsDir);
        }

        public async Task<IEnumerable<ProductResponseDto>> GetProductsAsync(string? search)
        {
            var query = _db.Products.Include(p => p.MediaAssets).AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.Name.Contains(search) || p.SKU.Contains(search));
            }

            return await query.Select(p => new ProductResponseDto
            {
                ProductId = p.ProductId, CategoryId = p.CategoryId,
                SKU = p.SKU, Name = p.Name, Brand = p.Brand, 
                Description = p.Description, PublishStatus = p.PublishStatus,
                Price = p.Price, StockQuantity = p.StockQuantity, WeightKg = p.WeightKg,
                DimensionsCm = p.DimensionsCm, Material = p.Material, Color = p.Color,
                WarrantyPeriod = p.WarrantyPeriod, Manufacturer = p.Manufacturer,
                Highlights = p.Highlights, HardwareInterface = p.HardwareInterface,
                Photos = p.MediaAssets.Select(m => m.Url).ToList()
            }).ToListAsync();
        }

        public async Task<ProductResponseDto?> GetProductByIdAsync(int id)
        {
            var p = await _db.Products.Include(p => p.MediaAssets).FirstOrDefaultAsync(p => p.ProductId == id);
            if (p == null) return null;
            return new ProductResponseDto { 
                ProductId = p.ProductId, CategoryId = p.CategoryId, SKU = p.SKU, Name = p.Name, Brand = p.Brand, Description = p.Description, PublishStatus = p.PublishStatus,
                Price = p.Price, StockQuantity = p.StockQuantity, WeightKg = p.WeightKg, DimensionsCm = p.DimensionsCm, Material = p.Material, Color = p.Color, WarrantyPeriod = p.WarrantyPeriod, Manufacturer = p.Manufacturer, Highlights = p.Highlights, HardwareInterface = p.HardwareInterface,
                Photos = p.MediaAssets.Select(m => m.Url).ToList(),
                MediaAssets = p.MediaAssets.Select(m => new MediaAssetDto { MediaId = m.MediaId, Url = m.Url }).ToList()
            };
        }

        public async Task<ProductResponseDto> CreateProductAsync(ProductCreateDto dto)
        {
            if (await _db.Products.AnyAsync(p => p.SKU == dto.SKU))
                throw new InvalidOperationException("Validation error blocks save."); // Matches TC05 expectation

            var product = new Product
            {
                CategoryId = dto.CategoryId, SKU = dto.SKU,
                Name = dto.Name, Brand = dto.Brand, Description = dto.Description,
                PublishStatus = "Draft", // Matches TC04 expectation
                Price = dto.Price, StockQuantity = dto.StockQuantity, WeightKg = dto.WeightKg, DimensionsCm = dto.DimensionsCm, Material = dto.Material, Color = dto.Color, WarrantyPeriod = dto.WarrantyPeriod, Manufacturer = dto.Manufacturer, Highlights = dto.Highlights, HardwareInterface = dto.HardwareInterface
            };

            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            return new ProductResponseDto { 
                ProductId = product.ProductId, CategoryId = product.CategoryId, SKU = product.SKU, Name = product.Name, Brand = product.Brand, Description = product.Description, PublishStatus = product.PublishStatus,
                Price = product.Price, StockQuantity = product.StockQuantity, WeightKg = product.WeightKg, DimensionsCm = product.DimensionsCm, Material = product.Material, Color = product.Color, WarrantyPeriod = product.WarrantyPeriod, Manufacturer = product.Manufacturer, Highlights = product.Highlights, HardwareInterface = product.HardwareInterface
            };
        }

        public async Task<bool> UpdateProductAsync(int id, ProductUpdateDto dto)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return false;

            // Check SKU uniqueness if changing
            if (product.SKU != dto.SKU && await _db.Products.AnyAsync(p => p.SKU == dto.SKU))
                throw new InvalidOperationException("Duplicate SKU.");

            product.CategoryId = dto.CategoryId;
            product.SKU = dto.SKU;
            product.Name = dto.Name;
            product.Brand = dto.Brand;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.StockQuantity = dto.StockQuantity;
            product.WeightKg = dto.WeightKg;
            product.DimensionsCm = dto.DimensionsCm;
            product.Material = dto.Material;
            product.Color = dto.Color;
            product.WarrantyPeriod = dto.WarrantyPeriod;
            product.Manufacturer = dto.Manufacturer;
            product.Highlights = dto.Highlights;
            product.HardwareInterface = dto.HardwareInterface;

            await _db.SaveChangesAsync();
            return true;
        }
        
        public async Task<bool> DeleteMediaAsync(int productId, int mediaId)
        {
            var media = await _db.MediaAssets.FirstOrDefaultAsync(m => m.MediaId == mediaId && m.ProductId == productId);
            if (media == null) return false;

            // Delete file from disk
            var filePath = Path.Combine(_uploadsDir, Path.GetFileName(media.Url));
            if (File.Exists(filePath)) File.Delete(filePath);

            _db.MediaAssets.Remove(media);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<int> DeleteAllMediaAsync(int productId)
        {
            var assets = await _db.MediaAssets.Where(m => m.ProductId == productId).ToListAsync();
            foreach (var asset in assets)
            {
                var filePath = Path.Combine(_uploadsDir, Path.GetFileName(asset.Url));
                if (File.Exists(filePath)) File.Delete(filePath);
            }
            _db.MediaAssets.RemoveRange(assets);
            await _db.SaveChangesAsync();
            return assets.Count;
        }

        public async Task<bool> UploadMediaAsync(int productId, MediaUploadDto dto)
        {
            var log = new System.Text.StringBuilder();
            log.AppendLine("================================================");
            log.AppendLine($"UPLOAD ATTEMPT — {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            log.AppendLine($"ProductId     : {productId}");
            log.AppendLine($"FileName      : {dto.FileName}");
            log.AppendLine($"Base64 length : {dto.Base64Content?.Length ?? 0} chars");

            try
            {
                // 1. Validate extension
                var validExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var ext = Path.GetExtension(dto.FileName ?? "").ToLower();
                log.AppendLine($"Extension     : '{ext}'");

                if (!validExtensions.Contains(ext))
                {
                    log.AppendLine("RESULT: REJECTED — invalid extension");
                    await WriteLogAsync(log.ToString());
                    throw new ArgumentException("File rejected with message");
                }

                // 2. Check product exists
                var product = await _db.Products.FindAsync(productId);
                log.AppendLine($"Product found : {(product != null ? "YES — " + product.Name : "NO")}");

                if (product == null)
                {
                    log.AppendLine("RESULT: FAILED — product not found");
                    await WriteLogAsync(log.ToString());
                    return false;
                }

                // 3. Validate base64
                if (string.IsNullOrWhiteSpace(dto.Base64Content))
                {
                    log.AppendLine("RESULT: FAILED — Base64Content is empty");
                    await WriteLogAsync(log.ToString());
                    return false;
                }

                // 4. Decode bytes
                byte[] bytes;
                try
                {
                    bytes = Convert.FromBase64String(dto.Base64Content);
                    log.AppendLine($"Decoded bytes : {bytes.Length} bytes");
                }
                catch (Exception ex)
                {
                    log.AppendLine($"RESULT: FAILED — Base64 decode error: {ex.Message}");
                    await WriteLogAsync(log.ToString());
                    return false;
                }

                // 5. Resolve uploads directory
                var uploadsDir = _uploadsDir;
                log.AppendLine($"Uploads dir   : {uploadsDir}");

                Directory.CreateDirectory(uploadsDir);
                log.AppendLine($"Dir exists    : {Directory.Exists(uploadsDir)}");

                // 6. Write file
                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsDir, fileName);
                await File.WriteAllBytesAsync(filePath, bytes);

                var fileExists = File.Exists(filePath);
                var fileSize = fileExists ? new FileInfo(filePath).Length : 0;
                log.AppendLine($"File written  : {filePath}");
                log.AppendLine($"File on disk  : {fileExists} ({fileSize} bytes)");

                // 7. Save to DB
                var media = new MediaAsset
                {
                    ProductId = productId,
                    Url = $"/uploads/{fileName}",
                    AssetType = "Image"
                };
                _db.MediaAssets.Add(media);
                await _db.SaveChangesAsync();

                // 8. Verify DB record
                var saved = await _db.MediaAssets.FirstOrDefaultAsync(m => m.Url == media.Url);
                log.AppendLine($"DB record     : MediaId={saved?.MediaId}, Url={saved?.Url}");
                log.AppendLine($"RESULT: SUCCESS — URL: /uploads/{fileName}");

                await WriteLogAsync(log.ToString());
                return true;
            }
            catch (ArgumentException)
            {
                throw; // re-throw validation errors
            }
            catch (Exception ex)
            {
                log.AppendLine($"RESULT: EXCEPTION — {ex.GetType().Name}: {ex.Message}");
                log.AppendLine($"Stack: {ex.StackTrace}");
                await WriteLogAsync(log.ToString());
                throw;
            }
        }

        private async Task WriteLogAsync(string content)
        {
            try
            {
                var dir = Path.GetDirectoryName(_uploadLogPath);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

                await _logLock.WaitAsync();
                try { await File.AppendAllTextAsync(_uploadLogPath, content + Environment.NewLine); }
                finally { _logLock.Release(); }
            }
            catch { /* log write failure must never crash the upload */ }
        }
    }
}
