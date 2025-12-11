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

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    /// <summary>
    /// Get product reviews
    /// </summary>
    [HttpGet("products/{productId}")]
    public async Task<IActionResult> GetProductReviews(Guid productId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _reviewService.GetProductReviewsAsync(productId, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get service reviews
    /// </summary>
    [HttpGet("services/{serviceId}")]
    public async Task<IActionResult> GetServiceReviews(Guid serviceId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _reviewService.GetServiceReviewsAsync(serviceId, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get product average rating
    /// </summary>
    [HttpGet("products/{productId}/rating")]
    public async Task<IActionResult> GetProductRating(Guid productId)
    {
        var result = await _reviewService.GetProductAverageRatingAsync(productId);
        return Ok(result);
    }

    /// <summary>
    /// Get service average rating
    /// </summary>
    [HttpGet("services/{serviceId}/rating")]
    public async Task<IActionResult> GetServiceRating(Guid serviceId)
    {
        var result = await _reviewService.GetServiceAverageRatingAsync(serviceId);
        return Ok(result);
    }

    /// <summary>
    /// Create product review (Authorized users)
    /// </summary>
    [HttpPost("products")]
    [Authorize]
    public async Task<IActionResult> CreateProductReview([FromBody] CreateProductReviewDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var result = await _reviewService.CreateProductReviewAsync(dto.ProductId, userId, dto.Rating, dto.Comment);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Create service review (Authorized users)
    /// </summary>
    [HttpPost("services")]
    [Authorize]
    public async Task<IActionResult> CreateServiceReview([FromBody] CreateServiceReviewDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var result = await _reviewService.CreateServiceReviewAsync(dto.AppointmentId, userId, dto.Rating, dto.Comment);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Delete product review (User can only delete their own)
    /// </summary>
    [HttpDelete("products/{reviewId}")]
    [Authorize]
    public async Task<IActionResult> DeleteProductReview(Guid reviewId)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var result = await _reviewService.DeleteProductReviewAsync(reviewId, userId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Delete service review (User can only delete their own)
    /// </summary>
    [HttpDelete("services/{reviewId}")]
    [Authorize]
    public async Task<IActionResult> DeleteServiceReview(Guid reviewId)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var result = await _reviewService.DeleteServiceReviewAsync(reviewId, userId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
