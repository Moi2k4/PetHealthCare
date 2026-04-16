using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCare.Application.DTOs.Health;
using PetCare.Application.Common;
using PetCare.Application.Services.Interfaces;
using PetCare.Domain.Entities;
using PetCare.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace PetCare.API.Controllers;

[ApiController]
[Route("api/health-records")]
[Authorize]
public class HealthRecordsController : ControllerBase
{
    private readonly IHealthRecordService _healthRecordService;
    private readonly PetCareDbContext _dbContext;

    public HealthRecordsController(IHealthRecordService healthRecordService, PetCareDbContext dbContext)
    {
        _healthRecordService = healthRecordService;
        _dbContext = dbContext;
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
    /// Update vaccination reminder status: booked, done, or remind_later.
    /// </summary>
    [HttpPost("pet/{petId}/vaccinations/{vaccinationId}/reminder-status")]
    public async Task<IActionResult> UpdateVaccinationReminderStatus(
        Guid petId,
        Guid vaccinationId,
        [FromBody] UpdateVaccinationReminderStatusRequest request)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var status = (request.Status ?? string.Empty).Trim().ToLowerInvariant();
        var allowed = new[] { "booked", "done", "remind_later" };
        if (!allowed.Contains(status))
        {
            return BadRequest(ServiceResult<object>.FailureResult("Invalid status. Use booked, done, or remind_later."));
        }

        var vaccination = await _dbContext.Vaccinations
            .Include(v => v.Pet)
            .FirstOrDefaultAsync(v => v.Id == vaccinationId && v.PetId == petId);

        if (vaccination == null)
        {
            return NotFound(ServiceResult<object>.FailureResult("Vaccination not found."));
        }

        if (vaccination.Pet.UserId != userId)
        {
            return Forbid();
        }

        var notePayload = JsonSerializer.Serialize(new
        {
            vaccinationId,
            status,
            note = request.Note,
            updatedBy = userId,
            updatedAt = DateTime.UtcNow
        });

        var log = new HealthReminder
        {
            Id = Guid.NewGuid(),
            PetId = petId,
            ReminderType = "vaccination_user_action",
            ReminderTitle = $"Vaccination reminder marked as {status}",
            ReminderDate = DateTime.UtcNow,
            IsCompleted = status == "done",
            Notes = notePayload,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.HealthReminders.AddAsync(log);
        await _dbContext.SaveChangesAsync();

        var response = ServiceResult<object>.SuccessResult(new
        {
            petId,
            vaccinationId,
            status
        }, "Vaccination reminder status updated.");

        return Ok(response);
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

public class UpdateVaccinationReminderStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string? Note { get; set; }
}
