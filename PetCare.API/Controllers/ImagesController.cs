using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCare.Application.Services.Interfaces;

namespace PetCare.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for all endpoints
public class ImagesController : ControllerBase
{
    private readonly IImageUploadService _imageUploadService;
    private readonly ILogger<ImagesController> _logger;

    public ImagesController(
        IImageUploadService imageUploadService,
        ILogger<ImagesController> logger)
    {
        _imageUploadService = imageUploadService;
        _logger = logger;
    }

    /// <summary>
    /// Upload a single image
    /// </summary>
    /// <param name="file">The image file to upload</param>
    /// <param name="folder">Optional folder name for organizing images (e.g., "pets", "products", "users")</param>
    /// <returns>The URL of the uploaded image</returns>
    [HttpPost("upload")]
    [RequestSizeLimit(5 * 1024 * 1024)] // 5MB limit
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UploadImage(
        [FromForm] IFormFile file,
        [FromForm] string folder = "petcare")
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file provided" });
        }

        var result = await _imageUploadService.UploadImageAsync(file, folder);

        if (result.Success)
        {
            return Ok(new { imageUrl = result.Data, message = "Image uploaded successfully" });
        }

        return BadRequest(new { message = result.Message });
    }

    /// <summary>
    /// Upload multiple images
    /// </summary>
    /// <param name="files">The image files to upload</param>
    /// <param name="folder">Optional folder name for organizing images</param>
    /// <returns>List of URLs of the uploaded images</returns>
    [HttpPost("upload-multiple")]
    [RequestSizeLimit(20 * 1024 * 1024)] // 20MB limit for multiple files
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UploadMultipleImages(
        [FromForm] List<IFormFile> files,
        [FromForm] string folder = "petcare")
    {
        if (files == null || !files.Any())
        {
            return BadRequest(new { message = "No files provided" });
        }

        if (files.Count > 10)
        {
            return BadRequest(new { message = "Maximum 10 files allowed per upload" });
        }

        var result = await _imageUploadService.UploadImagesAsync(files, folder);

        if (result.Success)
        {
            return Ok(new { imageUrls = result.Data, message = $"{result.Data!.Count} images uploaded successfully" });
        }

        return BadRequest(new { message = result.Message });
    }

    /// <summary>
    /// Delete an image
    /// </summary>
    /// <param name="imageUrl">The URL of the image to delete</param>
    /// <returns>Success status</returns>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteImage([FromQuery] string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl))
        {
            return BadRequest(new { message = "Image URL is required" });
        }

        var result = await _imageUploadService.DeleteImageAsync(imageUrl);

        if (result.Success)
        {
            return Ok(new { message = "Image deleted successfully" });
        }

        return BadRequest(new { message = result.Message });
    }

    /// <summary>
    /// Get an optimized version of an image URL with specified dimensions and quality
    /// </summary>
    /// <param name="imageUrl">The original image URL</param>
    /// <param name="width">Target width in pixels</param>
    /// <param name="height">Target height in pixels</param>
    /// <param name="quality">Image quality (1-100, default 80)</param>
    /// <returns>Optimized image URL</returns>
    [HttpGet("optimize")]
    [AllowAnonymous] // Allow public access for getting optimized URLs
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetOptimizedImageUrl(
        [FromQuery] string imageUrl,
        [FromQuery] int? width = null,
        [FromQuery] int? height = null,
        [FromQuery] int quality = 80)
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
        return Ok(new { optimizedUrl });
    }

    /// <summary>
    /// Validate image file without uploading
    /// </summary>
    /// <param name="file">The image file to validate</param>
    /// <returns>Validation result</returns>
    [HttpPost("validate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult ValidateImage([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file provided" });
        }

        var result = _imageUploadService.ValidateImage(file);

        if (result.Success)
        {
            return Ok(new 
            { 
                message = "Image is valid",
                fileName = file.FileName,
                fileSize = file.Length,
                contentType = file.ContentType
            });
        }

        return BadRequest(new { message = result.Message });
    }
}
