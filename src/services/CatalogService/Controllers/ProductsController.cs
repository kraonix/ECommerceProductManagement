using CatalogService.DTOs;
using CatalogService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
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
                return success ? Ok() : NotFound();
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }
    }
}
