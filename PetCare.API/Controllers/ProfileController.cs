using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCare.Application.DTOs.User;
using PetCare.Application.Services.Interfaces;

namespace PetCare.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // All endpoints require authentication
public class ProfileController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IImageUploadService _imageUploadService;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(
        IUserService userService,
        IImageUploadService imageUploadService,
        ILogger<ProfileController> logger)
    {
        _userService = userService;
        _imageUploadService = imageUploadService;
        _logger = logger;
    }

    /// <summary>
    /// Get current user's profile
    /// </summary>
    /// <returns>User profile information</returns>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = GetUserId();
        var result = await _userService.GetMyProfileAsync(userId);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Update current user's profile
    /// </summary>
    /// <param name="updateProfileDto">Profile update data</param>
    /// <returns>Updated profile</returns>
    [HttpPut("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileDto updateProfileDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetUserId();
        var result = await _userService.UpdateMyProfileAsync(userId, updateProfileDto);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Change current user's password
    /// </summary>
    /// <param name="changePasswordDto">Password change data</param>
    /// <returns>Success status</returns>
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetUserId();
        var result = await _userService.ChangePasswordAsync(userId, changePasswordDto);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Upload profile avatar/picture and update profile immediately
    /// </summary>
    /// <param name="file">Avatar image file</param>
    /// <returns>Updated profile with new avatar URL</returns>
    [HttpPost("avatar")]
    [RequestSizeLimit(5 * 1024 * 1024)] // 5MB limit
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file provided" });
        }

        var userId = GetUserId();

        // Upload image to Cloudinary or local storage
        var uploadResult = await _imageUploadService.UploadImageAsync(file, "users/avatars");
        
        if (!uploadResult.Success)
        {
            return BadRequest(new { message = uploadResult.Message });
        }

        // Get old avatar URL to delete later
        var oldProfile = await _userService.GetMyProfileAsync(userId);
        var oldAvatarUrl = oldProfile.Data?.AvatarUrl;

        // Update user's avatar URL
        var result = await _userService.UploadAvatarAsync(userId, uploadResult.Data!);

        if (!result.Success)
        {
            // If user update fails, delete the newly uploaded image
            await _imageUploadService.DeleteImageAsync(uploadResult.Data!);
            return BadRequest(result);
        }

        // Delete old avatar if it exists (cleanup)
        if (!string.IsNullOrEmpty(oldAvatarUrl))
        {
            await _imageUploadService.DeleteImageAsync(oldAvatarUrl);
        }

        // Return updated profile
        var profileResult = await _userService.GetMyProfileAsync(userId);
        return Ok(new 
        { 
            success = true,
            message = "Avatar uploaded successfully",
            data = new
            {
                avatarUrl = uploadResult.Data,
                // Generate optimized versions
                avatarUrlThumbnail = _imageUploadService.GetOptimizedImageUrl(uploadResult.Data!, 150, 150, 85),
                avatarUrlMedium = _imageUploadService.GetOptimizedImageUrl(uploadResult.Data!, 400, 400, 90),
                profile = profileResult.Data
            }
        });
    }

    /// <summary>
    /// Upload an image and get URL only (doesn't update profile)
    /// Useful for previewing before saving or uploading temporary images
    /// </summary>
    /// <param name="file">Image file</param>
    /// <param name="folder">Optional folder name (default: users/temp)</param>
    /// <returns>Image URL with optimized versions</returns>
    [HttpPost("upload-image")]
    [RequestSizeLimit(5 * 1024 * 1024)] // 5MB limit
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UploadImage(
        IFormFile file,
        string folder = "users/temp")
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file provided" });
        }

        // Validate the image
        var validationResult = _imageUploadService.ValidateImage(file);
        if (!validationResult.Success)
        {
            return BadRequest(new { message = validationResult.Message });
        }

        // Upload image
        var uploadResult = await _imageUploadService.UploadImageAsync(file, folder);
        
        if (!uploadResult.Success)
        {
            return BadRequest(new { message = uploadResult.Message });
        }

        var imageUrl = uploadResult.Data!;

        return Ok(new 
        { 
            success = true,
            message = "Image uploaded successfully",
            data = new
            {
                originalUrl = imageUrl,
                // Pre-generate different sizes for common use cases
                thumbnail = _imageUploadService.GetOptimizedImageUrl(imageUrl, 150, 150, 80),
                small = _imageUploadService.GetOptimizedImageUrl(imageUrl, 300, 300, 85),
                medium = _imageUploadService.GetOptimizedImageUrl(imageUrl, 600, 600, 90),
                large = _imageUploadService.GetOptimizedImageUrl(imageUrl, 1200, 1200, 90)
            }
        });
    }

    /// <summary>
    /// Get optimized image URL with custom dimensions
    /// </summary>
    /// <param name="imageUrl">Original image URL</param>
    /// <param name="width">Target width in pixels</param>
    /// <param name="height">Target height in pixels</param>
    /// <param name="quality">Quality (1-100, default 85)</param>
    /// <returns>Optimized image URL</returns>
    [HttpGet("optimize-image")]
    [AllowAnonymous] // Allow public access for optimization
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetOptimizedImageUrl(
        [FromQuery] string imageUrl,
        [FromQuery] int? width = null,
        [FromQuery] int? height = null,
        [FromQuery] int quality = 85)
    {
        if (string.IsNullOrEmpty(imageUrl))
        {
            return BadRequest(new { message = "Image URL is required" });
        }

        if (quality < 1 || quality > 100)
        {
            return BadRequest(new { message = "Quality must be between 1 and 100" });
        }

        var optimizedUrl = _imageUploadService.GetOptimizedImageUrl(imageUrl, width, height, quality);
        
        return Ok(new 
        { 
            success = true,
            data = new
            {
                originalUrl = imageUrl,
                optimizedUrl = optimizedUrl,
                width = width,
                height = height,
                quality = quality
            }
        });
    }

    /// <summary>
    /// Delete current user's avatar
    /// </summary>
    /// <returns>Success status</returns>
    [HttpDelete("avatar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteAvatar()
    {
        var userId = GetUserId();
        
        // Get current profile to find avatar URL
        var profileResult = await _userService.GetMyProfileAsync(userId);
        if (!profileResult.Success || string.IsNullOrEmpty(profileResult.Data?.AvatarUrl))
        {
            return Ok(new { message = "No avatar to delete" });
        }

        // Delete image from storage
        await _imageUploadService.DeleteImageAsync(profileResult.Data.AvatarUrl);

        // Update user profile to remove avatar URL
        var result = await _userService.UploadAvatarAsync(userId, string.Empty);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(new { message = "Avatar deleted successfully" });
    }

    /// <summary>
    /// Helper method to get user ID from JWT claims
    /// </summary>
    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID in token");
        }

        return userId;
    }
}
