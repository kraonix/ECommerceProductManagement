using CatalogService.Data;
using CatalogService.DTOs;
using CatalogService.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogService.Services
{
    public class ProductService
    {
        private readonly CatalogDbContext _db;    
        
        public ProductService(CatalogDbContext db) => _db = db;

        public async Task<IEnumerable<ProductResponseDto>> GetProductsAsync(string? search)
        {
            var query = _db.Products.AsQueryable();
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
                Highlights = p.Highlights, HardwareInterface = p.HardwareInterface
            }).ToListAsync();
        }

        public async Task<ProductResponseDto?> GetProductByIdAsync(int id)
        {
            var p = await _db.Products.FindAsync(id);
            if (p == null) return null;
            return new ProductResponseDto { 
                ProductId = p.ProductId, CategoryId = p.CategoryId, SKU = p.SKU, Name = p.Name, Brand = p.Brand, Description = p.Description, PublishStatus = p.PublishStatus,
                Price = p.Price, StockQuantity = p.StockQuantity, WeightKg = p.WeightKg, DimensionsCm = p.DimensionsCm, Material = p.Material, Color = p.Color, WarrantyPeriod = p.WarrantyPeriod, Manufacturer = p.Manufacturer, Highlights = p.Highlights, HardwareInterface = p.HardwareInterface
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
        
        public async Task<bool> UploadMediaAsync(int productId, MediaUploadDto dto)
        {
            var validExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var ext = System.IO.Path.GetExtension(dto.FileName).ToLower();
            if (!validExtensions.Contains(ext)) throw new ArgumentException("File rejected with message"); // Matches TC06 expectation

            var product = await _db.Products.FindAsync(productId);
            if (product == null) return false;

            var media = new MediaAsset { ProductId = productId, Url = $"/uploads/{Guid.NewGuid()}{ext}", AssetType = "Image" };
            _db.MediaAssets.Add(media);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
