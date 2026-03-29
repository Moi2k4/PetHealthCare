using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCare.Application.DTOs.Health;
using PetCare.Application.Services.Interfaces;

namespace PetCare.API.Controllers;

[ApiController]
[Route("api/health-records")]
[Authorize]
public class HealthRecordsController : ControllerBase
{
    private readonly IHealthRecordService _healthRecordService;

    public HealthRecordsController(IHealthRecordService healthRecordService)
    {
        _healthRecordService = healthRecordService;
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    /// <summary>
    /// Get active vaccine catalog options for standardized selection.
    /// </summary>
    [HttpGet("vaccine-catalog")]
    [AllowAnonymous]
    public async Task<IActionResult> GetVaccineCatalog()
    {
        var result = await _healthRecordService.GetVaccineCatalogAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get all health records for a pet
    /// </summary>
    [HttpGet("pet/{petId}")]
    public async Task<IActionResult> GetByPet(Guid petId)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _healthRecordService.GetByPetAsync(petId, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get routine dog vaccination and deworming schedule for a pet.
    /// </summary>
    [HttpGet("pet/{petId}/dog-routine")]
    public async Task<IActionResult> GetDogRoutine(Guid petId)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _healthRecordService.GetDogRoutineScheduleAsync(petId, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get vaccination history for a pet.
    /// </summary>
    [HttpGet("pet/{petId}/vaccinations")]
    public async Task<IActionResult> GetVaccinationsByPet(Guid petId)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _healthRecordService.GetVaccinationsByPetAsync(petId, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Record a completed vaccination for a pet (owner, staff, admin).
    /// </summary>
    [HttpPost("pet/{petId}/vaccinations")]
    public async Task<IActionResult> AddVaccination(Guid petId, [FromBody] CreateVaccinationDto dto)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _healthRecordService.AddVaccinationAsync(petId, dto, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get a single health record by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _healthRecordService.GetByIdAsync(id, userId);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// Create a new health record (owner, staff, admin)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateHealthRecordDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _healthRecordService.CreateAsync(dto, userId);
        if (!result.Success) return BadRequest(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update an existing health record (owner, staff, admin)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateHealthRecordDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _healthRecordService.UpdateAsync(id, dto, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete a health record (owner, staff, admin)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _healthRecordService.DeleteAsync(id, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
