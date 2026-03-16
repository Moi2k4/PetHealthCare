using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCare.Application.DTOs.Subscription;
using PetCare.Application.Services.Interfaces;

namespace PetCare.API.Controllers;

[ApiController]
[Route("api/subscriptions")]
public class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    /// <summary>
    /// Get all available subscription packages.
    /// </summary>
    [HttpGet("packages")]
    public async Task<IActionResult> GetPackages()
    {
        var result = await _subscriptionService.GetPackagesAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Create a PayOS payment link for a subscription package.
    /// Returns a checkout URL + QR code the user can pay with.
    /// </summary>
    [HttpPost("create-payment")]
    [Authorize]
    public async Task<IActionResult> CreatePayment([FromBody] CreateSubscriptionPaymentDto dto)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _subscriptionService.CreatePaymentLinkAsync(dto, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// PayOS webhook endpoint – called automatically by PayOS on payment completion.
    /// Register this URL in the PayOS dashboard.
    /// </summary>
    [HttpPost("payos-webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> PayOSWebhook([FromBody] PayOSWebhookDto webhook)
    {
        var result = await _subscriptionService.HandlePayOSWebhookAsync(webhook);
        // PayOS expects HTTP 200 even for "not a success payment" cases
        return Ok(new { success = result.Success, message = result.Message });
    }

    /// <summary>
    /// Confirm subscription payment from return URL flow (fallback when webhook is delayed).
    /// </summary>
    [HttpPost("confirm-payment")]
    [Authorize]
    public async Task<IActionResult> ConfirmPayment([FromQuery] long orderCode)
    {
        if (orderCode <= 0) return BadRequest(new { success = false, message = "Invalid orderCode" });

        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _subscriptionService.ConfirmPaymentAsync(orderCode, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get the current user's active subscription.
    /// </summary>
    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMySubscription()
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _subscriptionService.GetMySubscriptionAsync(userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Check whether the current user's account has an active membership.
    /// </summary>
    [HttpGet("membership-status")]
    [Authorize]
    public async Task<IActionResult> GetMembershipStatus()
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _subscriptionService.GetMembershipStatusAsync(userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Cancel the current user's active subscription.
    /// </summary>
    [HttpDelete("cancel")]
    [Authorize]
    public async Task<IActionResult> Cancel()
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _subscriptionService.CancelSubscriptionAsync(userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
