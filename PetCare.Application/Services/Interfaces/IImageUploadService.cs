using Microsoft.AspNetCore.Http;
using PetCare.Application.Common;

namespace PetCare.Application.Services.Interfaces;

/// <summary>
/// Service for handling image uploads to cloud storage (Cloudinary) or local storage
/// Designed to be easily swappable with other providers
/// </summary>
public interface IImageUploadService
{
    /// <summary>
    /// Upload a single image and return the URL wrapped in ServiceResult
    /// </summary>
    Task<ServiceResult<string>> UploadImageAsync(IFormFile file, string folder = "general");
    
    /// <summary>
    /// Upload multiple images and return URLs wrapped in ServiceResult
    /// </summary>
    Task<ServiceResult<List<string>>> UploadImagesAsync(IEnumerable<IFormFile> files, string folder = "general");
    
    /// <summary>
    /// Delete an image by URL or public ID, returns bool wrapped in ServiceResult
    /// </summary>
    Task<ServiceResult<bool>> DeleteImageAsync(string imageUrl);
    
    /// <summary>
    /// Validate image file (type, size, etc.) and return validation result
    /// </summary>
    ServiceResult<bool> ValidateImage(IFormFile file);
    
    /// <summary>
    /// Generate unique filename from extension
    /// </summary>
    string GenerateUniqueFileName(string extension);
    
    /// <summary>
    /// Get optimized image URL with transformations (resize, quality)
    /// </summary>
    string GetOptimizedImageUrl(string imageUrl, int? width = null, int? height = null, int quality = 80);
}

