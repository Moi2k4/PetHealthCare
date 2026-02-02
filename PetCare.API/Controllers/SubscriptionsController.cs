using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCare.Application.DTOs.Subscription;
using PetCare.Application.Services.Interfaces;

namespace PetCare.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionsController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    /// <summary>
    /// Get all subscription packages
    /// </summary>
    [HttpGet("packages")]
    public async Task<IActionResult> GetAllPackages([FromQuery] bool activeOnly = true)
    {
        var result = await _subscriptionService.GetAllPackagesAsync(activeOnly);
        return Ok(result);
    }

    /// <summary>
    /// Get package by ID
    /// </summary>
    [HttpGet("packages/{id}")]
    public async Task<IActionResult> GetPackageById(Guid id)
    {
        var result = await _subscriptionService.GetPackageByIdAsync(id);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Create subscription package (Admin only)
    /// </summary>
    [HttpPost("packages")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> CreatePackage([FromBody] CreateSubscriptionPackageDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _subscriptionService.CreatePackageAsync(
            dto.Name,
            dto.Description,
            dto.Price,
            dto.BillingCycle,
            dto.Features);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Update subscription package (Admin only)
    /// </summary>
    [HttpPut("packages/{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdatePackage(Guid id, [FromBody] UpdateSubscriptionPackageDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _subscriptionService.UpdatePackageAsync(id, dto.Name, dto.Description, dto.Price);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Deactivate subscription package (Admin only)
    /// </summary>
    [HttpDelete("packages/{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeactivatePackage(Guid id)
    {
        var result = await _subscriptionService.DeactivatePackageAsync(id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Subscribe to a package (User)
    /// </summary>
    [HttpPost("subscribe")]
    [Authorize]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var result = await _subscriptionService.SubscribeAsync(userId, dto.PackageId, dto.PaymentMethod, dto.TransactionId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Cancel subscription (User)
    /// </summary>
    [HttpPost("{subscriptionId}/cancel")]
    [Authorize]
    public async Task<IActionResult> CancelSubscription(Guid subscriptionId)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var result = await _subscriptionService.CancelSubscriptionAsync(userId, subscriptionId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get user's active subscription
    /// </summary>
    [HttpGet("my-subscription")]
    [Authorize]
    public async Task<IActionResult> GetMySubscription()
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var result = await _subscriptionService.GetUserActiveSubscriptionAsync(userId);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Get user's subscription history
    /// </summary>
    [HttpGet("my-history")]
    [Authorize]
    public async Task<IActionResult> GetMyHistory()
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var result = await _subscriptionService.GetUserSubscriptionHistoryAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// Check if user has a specific feature
    /// </summary>
    [HttpGet("check-feature/{featureName}")]
    [Authorize]
    public async Task<IActionResult> CheckFeature(string featureName)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var result = await _subscriptionService.CheckUserHasFeatureAsync(userId, featureName);
        return Ok(result);
    }
}
