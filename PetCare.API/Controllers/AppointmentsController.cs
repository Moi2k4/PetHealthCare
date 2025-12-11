using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCare.Application.DTOs.Appointment;
using PetCare.Application.Services.Interfaces;
using System.Security.Claims;

namespace PetCare.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();
        var result = await _appointmentService.CreateAppointmentAsync(dto, userId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetAppointmentById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAppointment(Guid id, [FromBody] UpdateAppointmentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();
        var result = await _appointmentService.UpdateAppointmentAsync(id, dto, userId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelAppointment(Guid id, [FromBody] string cancellationReason)
    {
        var userId = GetCurrentUserId();
        var result = await _appointmentService.CancelAppointmentAsync(id, cancellationReason, userId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAppointmentById(Guid id)
    {
        var userId = GetCurrentUserId();
        var result = await _appointmentService.GetAppointmentByIdAsync(id, userId);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet("my-appointments")]
    public async Task<IActionResult> GetMyAppointments([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = GetCurrentUserId();
        var result = await _appointmentService.GetUserAppointmentsAsync(userId, page, pageSize);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet]
    [Authorize(Roles = "admin,service_provider,staff")]
    public async Task<IActionResult> GetAllAppointments([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _appointmentService.GetAllAppointmentsAsync(page, pageSize);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("{id}/assign-staff")]
    [Authorize(Roles = "admin,service_provider")]
    public async Task<IActionResult> AssignStaff(Guid id, [FromBody] Guid staffId)
    {
        var result = await _appointmentService.AssignStaffAsync(id, staffId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "admin,service_provider,staff")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] string status)
    {
        var userId = GetCurrentUserId();
        var result = await _appointmentService.UpdateStatusAsync(id, status, userId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("by-date")]
    [Authorize(Roles = "admin,service_provider,staff")]
    public async Task<IActionResult> GetAppointmentsByDate([FromQuery] DateTime date)
    {
        var result = await _appointmentService.GetAppointmentsByDateAsync(date);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("staff/{staffId}")]
    [Authorize(Roles = "admin,service_provider,staff")]
    public async Task<IActionResult> GetStaffAppointments(Guid staffId, [FromQuery] DateTime? date = null)
    {
        var result = await _appointmentService.GetStaffAppointmentsAsync(staffId, date);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
