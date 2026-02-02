namespace PetCare.API.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCare.Application.Services.Interfaces;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin,service_provider,staff")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("admin")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetAdminDashboard()
    {
        var result = await _dashboardService.GetAdminDashboardAsync();
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }
        return Ok(result.Data);
    }

    [HttpGet("user")]
    public async Task<IActionResult> GetUserDashboard()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid user token" });
        }

        var result = await _dashboardService.GetUserDashboardAsync(userId);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }
        return Ok(result.Data);
    }

    [HttpGet("revenue/{year}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetRevenueByMonth(int year)
    {
        var result = await _dashboardService.GetRevenueByMonthAsync(year);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }
        return Ok(result.Data);
    }
}
