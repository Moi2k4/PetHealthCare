using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCare.Application.Services.Interfaces;
using PetCare.Application.DTOs.Product;
using PetCare.Application.Common;

namespace PetCare.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication by default
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous] // Public read access
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _productService.GetProductByIdAsync(id);
        
        if (!result.Success)
        {
            return NotFound(result);
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Get all products with pagination
    /// </summary>
    [HttpGet]
    [AllowAnonymous] // Public read access
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _productService.GetProductsAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get products by category
    /// </summary>
    [HttpGet("category/{categoryId}")]
    [AllowAnonymous] // Public read access
    public async Task<IActionResult> GetByCategory(Guid categoryId)
    {
        var result = await _productService.GetProductsByCategoryAsync(categoryId);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Search products
    /// </summary>
    [HttpGet("search")]
    [AllowAnonymous] // Public read access
    public async Task<IActionResult> Search([FromQuery] string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return BadRequest("Search term is required");
        }

        var result = await _productService.SearchProductsAsync(searchTerm);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Get all active products
    /// </summary>
    [HttpGet("active")]
    [AllowAnonymous] // Public read access
    public async Task<IActionResult> GetActive()
    {
        var result = await _productService.GetActiveProductsAsync();
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }
    

    /// <summary>
    /// Create a new product (Admin/Staff only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Create([FromBody] CreateProductDto createProductDto)
    {
        var result = await _productService.CreateProductAsync(createProductDto);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update a product (Admin/Staff only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductDto updateProductDto)
    {
        var result = await _productService.UpdateProductAsync(id, updateProductDto);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Delete a product (Admin/Staff only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _productService.DeleteProductAsync(id);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Update stock quantity (Admin/Staff only)
    /// </summary>
    [HttpPut("{id}/stock")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> UpdateStock(Guid id, [FromBody] int quantity)
    {
        var result = await _productService.UpdateStockAsync(id, quantity);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Add an image to a product (Admin/Staff only)
    /// </summary>
    [HttpPost("{id}/images")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> AddImage(Guid id, [FromBody] AddProductImageRequest request)
    {
        var result = await _productService.AddProductImageAsync(id, request.ImageUrl);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Remove an image from a product (Admin/Staff only)
    /// </summary>
    [HttpDelete("{id}/images/{imageId}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> RemoveImage(Guid id, Guid imageId)
    {
        var result = await _productService.RemoveProductImageAsync(id, imageId);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }
}

public record AddProductImageRequest(string ImageUrl);
