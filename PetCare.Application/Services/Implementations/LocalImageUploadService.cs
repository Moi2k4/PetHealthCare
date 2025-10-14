using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using PetCare.Application.Common;
using PetCare.Application.Services.Interfaces;

namespace PetCare.Application.Services.Implementations;

/// <summary>
/// Local file system implementation of image upload service
/// </summary>
public class LocalImageUploadService : IImageUploadService
{
    private readonly ImageUploadSettings _settings;
    private readonly string _uploadPath;

    public LocalImageUploadService(IOptions<ImageUploadSettings> settings)
    {
        _settings = settings.Value;
        _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), _settings.LocalStoragePath);
        
        // Ensure upload directory exists
        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
    }

    public async Task<ServiceResult<string>> UploadImageAsync(IFormFile file, string folder = "general")
    {
        try
        {
            var validationResult = ValidateImage(file);
            if (!validationResult.Success)
            {
                return ServiceResult<string>.FailureResult(validationResult.Message);
            }

            // Create folder if doesn't exist
            var folderPath = Path.Combine(_uploadPath, folder);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Generate unique filename
            var fileName = GenerateUniqueFileName(Path.GetExtension(file.FileName));
            var filePath = Path.Combine(folderPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return URL
            var imageUrl = $"{_settings.BaseUrl}/{folder}/{fileName}";
            return ServiceResult<string>.SuccessResult(imageUrl);
        }
        catch (Exception ex)
        {
            return ServiceResult<string>.FailureResult($"Upload failed: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<string>>> UploadImagesAsync(IEnumerable<IFormFile> files, string folder = "general")
    {
        var urls = new List<string>();
        var errors = new List<string>();
        
        foreach (var file in files)
        {
            var result = await UploadImageAsync(file, folder);
            if (result.Success)
            {
                urls.Add(result.Data!);
            }
            else
            {
                errors.Add($"{file.FileName}: {result.Message}");
            }
        }

        if (errors.Any())
        {
            // If some uploads failed, delete the successful ones to maintain consistency
            foreach (var url in urls)
            {
                await DeleteImageAsync(url);
            }
            
            return ServiceResult<List<string>>.FailureResult($"Upload failed for some files: {string.Join(", ", errors)}");
        }
        
        return ServiceResult<List<string>>.SuccessResult(urls);
    }

    public Task<ServiceResult<bool>> DeleteImageAsync(string imageUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return Task.FromResult(ServiceResult<bool>.FailureResult("Image URL is required"));
            }

            // Extract relative path from URL
            var relativePath = imageUrl.Replace(_settings.BaseUrl + "/", "");
            var filePath = Path.Combine(_uploadPath, relativePath);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return Task.FromResult(ServiceResult<bool>.SuccessResult(true));
            }
            
            return Task.FromResult(ServiceResult<bool>.FailureResult("Image not found"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ServiceResult<bool>.FailureResult($"Deletion failed: {ex.Message}"));
        }
    }

    public ServiceResult<bool> ValidateImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return ServiceResult<bool>.FailureResult("File is empty or null");
        }

        if (file.Length > _settings.MaxFileSizeBytes)
        {
            return ServiceResult<bool>.FailureResult($"File size exceeds maximum limit of {_settings.MaxFileSizeBytes / 1024 / 1024}MB");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_settings.AllowedExtensions.Contains(extension))
        {
            return ServiceResult<bool>.FailureResult($"File extension {extension} is not allowed. Allowed: {string.Join(", ", _settings.AllowedExtensions)}");
        }

        // Validate content type
        var allowedContentTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        if (!allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            return ServiceResult<bool>.FailureResult("Invalid file content type");
        }

        return ServiceResult<bool>.SuccessResult(true);
    }

    public string GenerateUniqueFileName(string extension)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var guid = Guid.NewGuid().ToString("N")[..8]; // First 8 chars of GUID
        return $"{timestamp}_{guid}{extension.ToLowerInvariant()}";
    }

    public string GetOptimizedImageUrl(string imageUrl, int? width = null, int? height = null, int quality = 80)
    {
        // Local storage doesn't support on-the-fly optimization
        // Return original URL
        return imageUrl;
    }
}
