using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using PetCare.Application.Common;
using PetCare.Application.Services.Interfaces;

namespace PetCare.Application.Services.Implementations;

public class CloudinaryImageUploadService : IImageUploadService
{
    private readonly Cloudinary _cloudinary;
    private readonly ImageUploadSettings _settings;

    public CloudinaryImageUploadService(IConfiguration configuration)
    {
        // Read from environment variables first, then fall back to appsettings.json
        var cloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME") 
            ?? configuration["Cloudinary:CloudName"];
        var apiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY") 
            ?? configuration["Cloudinary:ApiKey"];
        var apiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET") 
            ?? configuration["Cloudinary:ApiSecret"];

        if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
        {
            throw new InvalidOperationException("Cloudinary credentials are not configured. Please set CLOUDINARY_CLOUD_NAME, CLOUDINARY_API_KEY, and CLOUDINARY_API_SECRET in your .env file.");
        }

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true; // Use HTTPS

        _settings = new ImageUploadSettings
        {
            MaxFileSizeBytes = 5 * 1024 * 1024, // 5MB default
            AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" }
        };
    }

    public async Task<ServiceResult<string>> UploadImageAsync(IFormFile file, string folder = "petcare")
    {
        try
        {
            // Validate image
            var validationResult = ValidateImage(file);
            if (!validationResult.Success)
            {
                return ServiceResult<string>.FailureResult(validationResult.Message);
            }

            // Generate unique filename
            var uniqueFileName = GenerateUniqueFileName(Path.GetExtension(file.FileName));

            // Upload to Cloudinary
            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(uniqueFileName, stream),
                Folder = folder,
                PublicId = Path.GetFileNameWithoutExtension(uniqueFileName),
                Overwrite = false,
                Transformation = new Transformation()
                    .Quality("auto:good") // Automatic quality optimization
                    .FetchFormat("auto") // Automatic format selection (WebP for supported browsers)
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return ServiceResult<string>.SuccessResult(uploadResult.SecureUrl.ToString());
            }

            return ServiceResult<string>.FailureResult($"Upload failed: {uploadResult.Error?.Message ?? "Unknown error"}");
        }
        catch (Exception ex)
        {
            return ServiceResult<string>.FailureResult($"Upload failed: {ex.Message}");
        }
    }

    public async Task<ServiceResult<string>> UploadImageFromUrlAsync(string imageUrl, string folder = "petcare")
    {
        try
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return ServiceResult<string>.FailureResult("Image URL is required");
            }

            // Generate unique filename 
            // We assume jpg extension if we can't determine it, Cloudinary handles format detection mostly
            var extension = Path.GetExtension(imageUrl);
            if (string.IsNullOrEmpty(extension) || extension.Length > 5) extension = ".jpg";
            
            var uniqueFileName = GenerateUniqueFileName(extension);

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(imageUrl),
                Folder = folder,
                PublicId = Path.GetFileNameWithoutExtension(uniqueFileName),
                Overwrite = false,
                Transformation = new Transformation()
                    .Quality("auto:good")
                    .FetchFormat("auto")
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return ServiceResult<string>.SuccessResult(uploadResult.SecureUrl.ToString());
            }

            return ServiceResult<string>.FailureResult($"Upload failed: {uploadResult.Error?.Message ?? "Unknown error"}");
        }
        catch (Exception ex)
        {
            return ServiceResult<string>.FailureResult($"Upload failed: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<string>>> UploadImagesAsync(IEnumerable<IFormFile> files, string folder = "petcare")
    {
        var uploadedUrls = new List<string>();
        var errors = new List<string>();

        foreach (var file in files)
        {
            var result = await UploadImageAsync(file, folder);
            
            if (result.Success)
            {
                uploadedUrls.Add(result.Data!);
            }
            else
            {
                errors.Add($"{file.FileName}: {result.Message}");
            }
        }

        if (errors.Any())
        {
            // If some uploads failed, delete the successful ones to maintain consistency
            foreach (var url in uploadedUrls)
            {
                await DeleteImageAsync(url);
            }
            
            return ServiceResult<List<string>>.FailureResult($"Upload failed for some files: {string.Join(", ", errors)}");
        }

        return ServiceResult<List<string>>.SuccessResult(uploadedUrls);
    }

    public async Task<ServiceResult<bool>> DeleteImageAsync(string imageUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return ServiceResult<bool>.FailureResult("Image URL is required");
            }

            // Extract public ID from Cloudinary URL
            var publicId = ExtractPublicIdFromUrl(imageUrl);
            if (string.IsNullOrEmpty(publicId))
            {
                return ServiceResult<bool>.FailureResult("Invalid Cloudinary URL");
            }

            var deletionParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Image
            };

            var deletionResult = await _cloudinary.DestroyAsync(deletionParams);

            if (deletionResult.Result == "ok" || deletionResult.Result == "not found")
            {
                return ServiceResult<bool>.SuccessResult(true);
            }

            return ServiceResult<bool>.FailureResult($"Deletion failed: {deletionResult.Error?.Message ?? "Unknown error"}");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult($"Deletion failed: {ex.Message}");
        }
    }

    public ServiceResult<bool> ValidateImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return ServiceResult<bool>.FailureResult("File is empty or null");
        }

        // Check file size
        if (file.Length > _settings.MaxFileSizeBytes)
        {
            return ServiceResult<bool>.FailureResult($"File size exceeds {_settings.MaxFileSizeBytes / 1024 / 1024}MB limit");
        }

        // Check file extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_settings.AllowedExtensions.Contains(extension))
        {
            return ServiceResult<bool>.FailureResult($"File type {extension} is not allowed. Allowed types: {string.Join(", ", _settings.AllowedExtensions)}");
        }

        // Validate content type
        if (!file.ContentType.StartsWith("image/"))
        {
            return ServiceResult<bool>.FailureResult("File must be an image");
        }

        return ServiceResult<bool>.SuccessResult(true);
    }

    public string GenerateUniqueFileName(string extension)
    {
        return $"{Guid.NewGuid()}{extension.ToLowerInvariant()}";
    }

    public string GetOptimizedImageUrl(string imageUrl, int? width = null, int? height = null, int quality = 80)
    {
        if (string.IsNullOrEmpty(imageUrl) || !imageUrl.Contains("cloudinary.com"))
        {
            return imageUrl;
        }

        try
        {
            // Build transformation string
            var transformations = new List<string>();

            if (width.HasValue && height.HasValue)
            {
                transformations.Add($"w_{width.Value},h_{height.Value},c_fill");
            }
            else if (width.HasValue)
            {
                transformations.Add($"w_{width.Value}");
            }
            else if (height.HasValue)
            {
                transformations.Add($"h_{height.Value}");
            }

            transformations.Add($"q_{quality}");
            transformations.Add("f_auto"); // Auto format (WebP when supported)

            var transformationString = string.Join(",", transformations);

            // Insert transformation into URL
            // Example: https://res.cloudinary.com/demo/image/upload/v1234567890/sample.jpg
            // Becomes: https://res.cloudinary.com/demo/image/upload/w_300,h_300,q_80,f_auto/v1234567890/sample.jpg
            var uploadIndex = imageUrl.IndexOf("/upload/", StringComparison.OrdinalIgnoreCase);
            if (uploadIndex > 0)
            {
                return imageUrl.Insert(uploadIndex + 8, $"{transformationString}/");
            }

            return imageUrl;
        }
        catch
        {
            return imageUrl;
        }
    }

    /// <summary>
    /// Extracts the public ID from a Cloudinary URL
    /// Example: https://res.cloudinary.com/demo/image/upload/v1234567890/folder/filename.jpg
    /// Returns: folder/filename
    /// </summary>
    private string ExtractPublicIdFromUrl(string imageUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(imageUrl) || !imageUrl.Contains("cloudinary.com"))
            {
                return string.Empty;
            }

            var uri = new Uri(imageUrl);
            var pathSegments = uri.AbsolutePath.Split('/').Where(s => !string.IsNullOrEmpty(s)).ToArray();

            // Find the index of "upload"
            var uploadIndex = Array.FindIndex(pathSegments, s => s.Equals("upload", StringComparison.OrdinalIgnoreCase));
            if (uploadIndex < 0 || uploadIndex >= pathSegments.Length - 1)
            {
                return string.Empty;
            }

            // Skip "upload" and version (v1234567890)
            var startIndex = uploadIndex + 1;
            if (pathSegments[startIndex].StartsWith("v") && long.TryParse(pathSegments[startIndex].Substring(1), out _))
            {
                startIndex++; // Skip version
            }

            // Get remaining segments and remove file extension
            var publicIdSegments = pathSegments.Skip(startIndex).ToArray();
            var publicId = string.Join("/", publicIdSegments);

            // Remove file extension
            var lastDotIndex = publicId.LastIndexOf('.');
            if (lastDotIndex > 0)
            {
                publicId = publicId.Substring(0, lastDotIndex);
            }

            return publicId;
        }
        catch
        {
            return string.Empty;
        }
    }
}

