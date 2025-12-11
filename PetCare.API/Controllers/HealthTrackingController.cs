using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCare.Application.DTOs.Health;
using PetCare.Application.Services.Interfaces;

namespace PetCare.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HealthTrackingController : ControllerBase
{
    private readonly IHealthTrackingService _healthTrackingService;

    public HealthTrackingController(IHealthTrackingService healthTrackingService)
    {
        _healthTrackingService = healthTrackingService;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    #region Health Records

    /// <summary>
    /// Get pet health records
    /// </summary>
    [HttpGet("records/pet/{petId}")]
    public async Task<IActionResult> GetPetHealthRecords(Guid petId)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var result = await _healthTrackingService.GetPetHealthRecordsAsync(petId, userId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Create health record
    /// </summary>
    [HttpPost("records")]
    public async Task<IActionResult> CreateHealthRecord([FromBody] CreateHealthRecordDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var result = await _healthTrackingService.CreateHealthRecordAsync(
            dto.PetId,
            userId,
            dto.RecordType,
            dto.Description,
            dto.RecordDate,
            dto.VeterinarianId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Delete health record
    /// </summary>
    [HttpDelete("records/{recordId}")]
    public async Task<IActionResult> DeleteHealthRecord(Guid recordId)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var result = await _healthTrackingService.DeleteHealthRecordAsync(recordId, userId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    #endregion

    #region Vaccinations

    /// <summary>
    /// Get pet vaccinations
    /// </summary>
    [HttpGet("vaccinations/pet/{petId}")]
    public async Task<IActionResult> GetPetVaccinations(Guid petId)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var result = await _healthTrackingService.GetPetVaccinationsAsync(petId, userId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get upcoming vaccinations
    /// </summary>
    [HttpGet("vaccinations/upcoming")]
    public async Task<IActionResult> GetUpcomingVaccinations([FromQuery] int daysAhead = 30)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var result = await _healthTrackingService.GetUpcomingVaccinationsAsync(userId, daysAhead);
        return Ok(result);
    }

    /// <summary>
    /// Add vaccination
    /// </summary>
    [HttpPost("vaccinations")]
    public async Task<IActionResult> AddVaccination([FromBody] CreateVaccinationDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var result = await _healthTrackingService.AddVaccinationAsync(
            dto.PetId,
            userId,
            dto.VaccineName,
            dto.VaccinationDate,
            dto.NextDueDate,
            dto.BatchNumber);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Delete vaccination
    /// </summary>
    [HttpDelete("vaccinations/{vaccinationId}")]
    public async Task<IActionResult> DeleteVaccination(Guid vaccinationId)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var result = await _healthTrackingService.DeleteVaccinationAsync(vaccinationId, userId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    #endregion

    #region Health Reminders

    /// <summary>
    /// Get pet reminders
    /// </summary>
    [HttpGet("reminders/pet/{petId}")]
    public async Task<IActionResult> GetPetReminders(Guid petId, [FromQuery] bool activeOnly = true)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var result = await _healthTrackingService.GetPetRemindersAsync(petId, userId, activeOnly);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get upcoming reminders
    /// </summary>
    [HttpGet("reminders/upcoming")]
    public async Task<IActionResult> GetUpcomingReminders([FromQuery] int daysAhead = 7)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var result = await _healthTrackingService.GetUpcomingRemindersAsync(userId, daysAhead);
        return Ok(result);
    }

    /// <summary>
    /// Create reminder
    /// </summary>
    [HttpPost("reminders")]
    public async Task<IActionResult> CreateReminder([FromBody] CreateHealthReminderDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var result = await _healthTrackingService.CreateReminderAsync(
            dto.PetId,
            userId,
            dto.ReminderType,
            dto.Title,
            dto.ReminderDate,
            dto.Notes);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Complete reminder
    /// </summary>
    [HttpPatch("reminders/{reminderId}/complete")]
    public async Task<IActionResult> CompleteReminder(Guid reminderId)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var result = await _healthTrackingService.CompleteReminderAsync(reminderId, userId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Delete reminder
    /// </summary>
    [HttpDelete("reminders/{reminderId}")]
    public async Task<IActionResult> DeleteReminder(Guid reminderId)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var result = await _healthTrackingService.DeleteReminderAsync(reminderId, userId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    #endregion
}
