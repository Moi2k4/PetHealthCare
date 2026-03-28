using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCare.Application.DTOs.Appointment;
using PetCare.Application.Services.Interfaces;

namespace PetCare.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    /// <summary>
    /// Get all active services available for booking (public)
    /// </summary>
    [HttpGet("services")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAvailableServices()
    {
        var result = await _appointmentService.GetAvailableServicesAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Customer creates a new appointment booking
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetUserId();
        var result = await _appointmentService.CreateAppointmentAsync(dto, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Customer views their own appointments
    /// </summary>
    [HttpGet("my-appointments")]
    [Authorize]
    public async Task<IActionResult> GetMyAppointments()
    {
        var userId = GetUserId();
        var result = await _appointmentService.GetUserAppointmentsAsync(userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get a single appointment by ID (customer sees own; doctor/admin see any)
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetAppointmentById(Guid id)
    {
        var userId = GetUserId();
        var userRole = GetUserRole();
        var result = await _appointmentService.GetAppointmentByIdAsync(id, userId, userRole);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Customer cancels their appointment
    /// </summary>
    [HttpPatch("{id:guid}/cancel")]
    [Authorize]
    public async Task<IActionResult> CancelAppointment(Guid id, [FromBody] CancelAppointmentRequest? request)
    {
        var userId = GetUserId();
        var result = await _appointmentService.CancelAppointmentAsync(id, userId, request?.CancellationReason);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Doctor / Admin views all appointments with optional status and date filters
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Doctor,doctor,Admin,admin,Provider,provider,ServiceProvider,serviceprovider,Service_Provider,service_provider")]
    public async Task<IActionResult> GetAllAppointments([FromQuery] string? status, [FromQuery] DateTime? date)
    {
        var result = await _appointmentService.GetAllAppointmentsAsync(status, date);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Doctor / Admin updates appointment status and adds medical notes
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Doctor,doctor,Admin,admin,Provider,provider,ServiceProvider,serviceprovider,Service_Provider,service_provider")]
    public async Task<IActionResult> UpdateAppointmentStatus(Guid id, [FromBody] UpdateAppointmentStatusDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var doctorId = GetUserId();
        var result = await _appointmentService.UpdateAppointmentStatusAsync(id, dto, doctorId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ─── Helpers ────────────────────────────────────────────────────────────

    private Guid GetUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? User.FindFirst("sub")?.Value;

        if (Guid.TryParse(idClaim, out var id)) return id;
        throw new UnauthorizedAccessException("Invalid user identity");
    }

    private string GetUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
    }
}

public class CancelAppointmentRequest
{
    public string? CancellationReason { get; set; }
}
