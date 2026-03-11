using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCare.Application.DTOs.Review;
using PetCare.Application.Services.Interfaces;

namespace PetCare.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    private Guid? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }

    // ──────────────────────── Product Reviews ────────────────────────

    /// <summary>Get approved reviews for a product</summary>
    [HttpGet("products/{productId:guid}")]
    public async Task<IActionResult> GetProductReviews(Guid productId)
    {
        var result = await _reviewService.GetProductReviewsAsync(productId);
        return Ok(result);
    }

    /// <summary>Get my product reviews</summary>
    [HttpGet("products/my")]
    [Authorize]
    public async Task<IActionResult> GetMyProductReviews()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await _reviewService.GetMyProductReviewsAsync(userId.Value);
        return Ok(result);
    }

    /// <summary>Submit a product review (authenticated, one per product)</summary>
    [HttpPost("products")]
    [Authorize]
    public async Task<IActionResult> CreateProductReview([FromBody] CreateProductReviewDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await _reviewService.CreateProductReviewAsync(userId.Value, dto);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>Approve a product review (admin)</summary>
    [HttpPatch("products/{reviewId:guid}/approve")]
    [Authorize]
    public async Task<IActionResult> ApproveProductReview(Guid reviewId)
    {
        var result = await _reviewService.ApproveProductReviewAsync(reviewId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>Delete a product review (admin)</summary>
    [HttpDelete("products/{reviewId:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteProductReview(Guid reviewId)
    {
        var result = await _reviewService.DeleteProductReviewAsync(reviewId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    // ──────────────────────── Service Reviews ────────────────────────

    /// <summary>Get approved reviews for a service</summary>
    [HttpGet("services/{serviceId:guid}")]
    public async Task<IActionResult> GetServiceReviews(Guid serviceId)
    {
        var result = await _reviewService.GetServiceReviewsAsync(serviceId);
        return Ok(result);
    }

    /// <summary>Get my service reviews</summary>
    [HttpGet("services/my")]
    [Authorize]
    public async Task<IActionResult> GetMyServiceReviews()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await _reviewService.GetMyServiceReviewsAsync(userId.Value);
        return Ok(result);
    }

    /// <summary>Submit a service review (authenticated, one per appointment)</summary>
    [HttpPost("services")]
    [Authorize]
    public async Task<IActionResult> CreateServiceReview([FromBody] CreateServiceReviewDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await _reviewService.CreateServiceReviewAsync(userId.Value, dto);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>Approve a service review (admin)</summary>
    [HttpPatch("services/{reviewId:guid}/approve")]
    [Authorize]
    public async Task<IActionResult> ApproveServiceReview(Guid reviewId)
    {
        var result = await _reviewService.ApproveServiceReviewAsync(reviewId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>Delete a service review (admin)</summary>
    [HttpDelete("services/{reviewId:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteServiceReview(Guid reviewId)
    {
        var result = await _reviewService.DeleteServiceReviewAsync(reviewId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}
