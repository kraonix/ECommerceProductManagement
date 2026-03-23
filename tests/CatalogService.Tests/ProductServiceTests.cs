using CatalogService.Data;
using CatalogService.DTOs;
using CatalogService.Services;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogService.Tests
{
    [TestFixture]
    public class ProductServiceTests
    {
        private CatalogDbContext _db = null!;
        private ProductService _service = null!;    
        
        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<CatalogDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            _db = new CatalogDbContext(options);
            _service = new ProductService(_db);
            
            _db.Categories.Add(new CatalogService.Entities.Category { CategoryId = 1, Name = "Electronics" });
            _db.SaveChanges();
        }

        [TearDown]
        public void TearDown() => _db.Dispose();

        [Test]
        public async Task SearchProduct_ByName_ReturnsMatches() // TC03
        {
            await _service.CreateProductAsync(new ProductCreateDto { CategoryId = 1, SKU = "SKU1", Name = "Apple iPhone" });
            await _service.CreateProductAsync(new ProductCreateDto { CategoryId = 1, SKU = "SKU2", Name = "Samsung Phone" });
            
            var results = await _service.GetProductsAsync("iPhone");
            Assert.That(results.Count(), Is.EqualTo(1));
            Assert.That(results.First().SKU, Is.EqualTo("SKU1"));
        }

        [Test]
        public async Task CreateProduct_Valid_ReturnsProductInDraftState() // TC04
        {
            var dto = new ProductCreateDto { CategoryId = 1, SKU = "SKU123", Name = "Laptop" };
            var result = await _service.CreateProductAsync(dto);
            Assert.That(result.SKU, Is.EqualTo("SKU123"));
            Assert.That(result.PublishStatus, Is.EqualTo("Draft"));
        }

        [Test]
        public async Task CreateProduct_DuplicateSKU_ThrowsException() // TC05
        {
            var dto = new ProductCreateDto { CategoryId = 1, SKU = "DUP123", Name = "Phone" };
            await _service.CreateProductAsync(dto);
            Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateProductAsync(dto));
        }

        [Test]
        public async Task UploadMedia_InvalidType_ThrowsArgumentException() // TC06
        {
            var dto = new ProductCreateDto { CategoryId = 1, SKU = "IMG123", Name = "Camera" };
            var product = await _service.CreateProductAsync(dto);
            
            var mediaDto = new MediaUploadDto { FileName = "document.pdf", Base64Content = "..." };
            Assert.ThrowsAsync<ArgumentException>(() => _service.UploadMediaAsync(product.ProductId, mediaDto));
        }
    }
}
