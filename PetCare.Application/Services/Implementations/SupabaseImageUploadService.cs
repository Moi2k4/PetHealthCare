using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using PetCare.Application.Common;
using PetCare.Application.Services.Interfaces;
using System.Net.Http.Headers;

namespace PetCare.Application.Services.Implementations;

/// <summary>
/// Supabase Storage implementation (deprecated - use Cloudinary or Local storage instead)
/// Kept for backward compatibility
/// </summary>
public class SupabaseImageUploadService : IImageUploadService
{
    private readonly HttpClient _httpClient;
    private readonly string _supabaseUrl;
    private readonly string _supabaseKey;
    private readonly string _bucketName;
    private readonly long _maxFileSizeBytes;
    private readonly string[] _allowedExtensions;

    public SupabaseImageUploadService(IConfiguration configuration)
    {
        _httpClient = new HttpClient();
        _supabaseUrl = Environment.GetEnvironmentVariable("SUPABASE_URL") 
            ?? configuration["Supabase:Url"] 
            ?? throw new InvalidOperationException("Supabase URL not configured");
        
        _supabaseKey = Environment.GetEnvironmentVariable("SUPABASE_SERVICE_ROLE_KEY") 
            ?? configuration["Supabase:ServiceRoleKey"] 
            ?? throw new InvalidOperationException("Supabase service role key not configured");
        
        _bucketName = Environment.GetEnvironmentVariable("SUPABASE_BUCKET_NAME") 
            ?? configuration["Supabase:BucketName"] 
            ?? "petcare-images";
        
        // 5MB max file size
        _maxFileSizeBytes = 5 * 1024 * 1024;
        _allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        
        _httpClient.DefaultRequestHeaders.Add("apikey", _supabaseKey);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_supabaseKey}");
    }

    public ServiceResult<bool> ValidateImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return ServiceResult<bool>.FailureResult("No file provided");
        }

        if (file.Length > _maxFileSizeBytes)
        {
            return ServiceResult<bool>.FailureResult($"File size exceeds maximum allowed size of {_maxFileSizeBytes / 1024 / 1024}MB");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
        {
            return ServiceResult<bool>.FailureResult($"Invalid file type. Allowed types: {string.Join(", ", _allowedExtensions)}");
        }

        return ServiceResult<bool>.SuccessResult(true);
    }

    public async Task<ServiceResult<string>> UploadImageAsync(IFormFile file, string folder = "general")
    {
        var validationResult = ValidateImage(file);
        if (!validationResult.Success)
        {
            return ServiceResult<string>.FailureResult(validationResult.Message);
        }

        try
        {
            // Generate unique filename
            var extension = Path.GetExtension(file.FileName);
            var fileName = GenerateUniqueFileName(extension);
            var filePath = $"{folder}/{fileName}";

            // Prepare file content
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();

            // Upload to Supabase Storage
            var content = new ByteArrayContent(fileBytes);
            content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

            var uploadUrl = $"{_supabaseUrl}/storage/v1/object/{_bucketName}/{filePath}";
            var response = await _httpClient.PostAsync(uploadUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return ServiceResult<string>.FailureResult($"Failed to upload image to Supabase: {error}");
            }

            // Return public URL
            var publicUrl = $"{_supabaseUrl}/storage/v1/object/public/{_bucketName}/{filePath}";
            return ServiceResult<string>.SuccessResult(publicUrl);
        }
        catch (Exception ex)
        {
            return ServiceResult<string>.FailureResult($"Error uploading image: {ex.Message}");
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
            // If some uploads failed, delete the successful ones
            foreach (var url in urls)
            {
                await DeleteImageAsync(url);
            }
            
            return ServiceResult<List<string>>.FailureResult($"Upload failed for some files: {string.Join(", ", errors)}");
        }

        return ServiceResult<List<string>>.SuccessResult(urls);
    }

    public async Task<ServiceResult<bool>> DeleteImageAsync(string imageUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(imageUrl))
                return ServiceResult<bool>.FailureResult("Image URL is required");

            // Extract file path from URL
            var publicPrefix = $"{_supabaseUrl}/storage/v1/object/public/{_bucketName}/";
            if (!imageUrl.StartsWith(publicPrefix))
                return ServiceResult<bool>.FailureResult("Invalid Supabase image URL");

            var filePath = imageUrl.Substring(publicPrefix.Length);

            // Delete from Supabase Storage
            var deleteUrl = $"{_supabaseUrl}/storage/v1/object/{_bucketName}/{filePath}";
            var response = await _httpClient.DeleteAsync(deleteUrl);

            if (response.IsSuccessStatusCode)
            {
                return ServiceResult<bool>.SuccessResult(true);
            }

            return ServiceResult<bool>.FailureResult("Failed to delete image from Supabase");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult($"Error deleting image: {ex.Message}");
        }
    }

    public string GenerateUniqueFileName(string extension)
    {
        return $"{Guid.NewGuid()}{extension.ToLowerInvariant()}";
    }

    public Task<ServiceResult<string>> UploadImageFromUrlAsync(string imageUrl, string folder = "general")
    {
        return Task.FromResult(ServiceResult<string>.FailureResult("Upload from URL is not supported by the Supabase provider"));
    }

    public string GetOptimizedImageUrl(string imageUrl, int? width = null, int? height = null, int quality = 80)
    {
        // Supabase Storage doesn't support on-the-fly transformations
        // Return original URL
        return imageUrl;
    }
}

