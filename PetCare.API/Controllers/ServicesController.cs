using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCare.Application.DTOs.Service;
using PetCare.Application.Services.Interfaces;

namespace PetCare.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServicesController : ControllerBase
{
    private readonly IServiceManagementService _serviceManagementService;

    public ServicesController(IServiceManagementService serviceManagementService)
    {
        _serviceManagementService = serviceManagementService;
    }

    /// <summary>
    /// Get service by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _serviceManagementService.GetServiceByIdAsync(id);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Get all services with pagination
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] bool activeOnly = true)
    {
        var result = await _serviceManagementService.GetAllServicesAsync(page, pageSize, activeOnly);
        return Ok(result);
    }

    /// <summary>
    /// Get services by category
    /// </summary>
    [HttpGet("category/{categoryId}")]
    public async Task<IActionResult> GetByCategory(Guid categoryId)
    {
        var result = await _serviceManagementService.GetServicesByCategoryAsync(categoryId);
        return Ok(result);
    }

    /// <summary>
    /// Create new service (Admin/Staff only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "admin,service_provider,staff")]
    public async Task<IActionResult> Create([FromBody] CreateServiceDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _serviceManagementService.CreateServiceAsync(
            dto.ServiceName,
            dto.Description ?? string.Empty,
            dto.CategoryId ?? Guid.Empty,
            dto.DurationMinutes,
            dto.Price,
            dto.IsHomeService);

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update service (Admin/Staff only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "admin,service_provider,staff")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateServiceDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _serviceManagementService.UpdateServiceAsync(
            id,
            dto.ServiceName,
            dto.Description,
            dto.Price,
            dto.DurationMinutes);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Delete (deactivate) service (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _serviceManagementService.DeleteServiceAsync(id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get all service categories
    /// </summary>
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var result = await _serviceManagementService.GetAllServiceCategoriesAsync();
        return Ok(result);
    }

    /// <summary>
    /// Create service category (Admin only)
    /// </summary>
    [HttpPost("categories")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateServiceCategoryDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _serviceManagementService.CreateServiceCategoryAsync(
            dto.CategoryName,
            dto.Description,
            dto.IconUrl);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
