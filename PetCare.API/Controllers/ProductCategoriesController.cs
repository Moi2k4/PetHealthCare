using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCare.Application.DTOs.Category;
using PetCare.Application.Services.Interfaces;

namespace PetCare.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductCategoriesController : ControllerBase
{
    private readonly IProductCategoryService _categoryService;

    public ProductCategoriesController(IProductCategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Get category by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _categoryService.GetCategoryByIdAsync(id);
        
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Get all categories
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var result = await _categoryService.GetAllCategoriesAsync(includeInactive);
        return Ok(result);
    }

    /// <summary>
    /// Get category hierarchy (tree structure)
    /// </summary>
    [HttpGet("hierarchy")]
    public async Task<IActionResult> GetHierarchy()
    {
        var result = await _categoryService.GetCategoryHierarchyAsync();
        return Ok(result);
    }

    /// <summary>
    /// Get subcategories of a parent category
    /// </summary>
    [HttpGet("{parentId}/subcategories")]
    public async Task<IActionResult> GetSubCategories(Guid parentId)
    {
        var result = await _categoryService.GetSubCategoriesAsync(parentId);
        return Ok(result);
    }

    /// <summary>
    /// Create new category (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Create([FromBody] CreateProductCategoryDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _categoryService.CreateCategoryAsync(dto);

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update category (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductCategoryDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _categoryService.UpdateCategoryAsync(id, dto);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Delete category (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _categoryService.DeleteCategoryAsync(id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Toggle category active status (Admin only)
    /// </summary>
    [HttpPatch("{id}/toggle-active")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> ToggleActive(Guid id)
    {
        var result = await _categoryService.ToggleActiveStatusAsync(id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
