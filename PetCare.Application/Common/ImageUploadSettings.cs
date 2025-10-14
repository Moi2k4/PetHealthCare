namespace PetCare.Application.Common;

/// <summary>
/// Settings for image upload configuration
/// </summary>
public class ImageUploadSettings
{
    public string StorageType { get; set; } = "Local"; // Local, S3, Azure, Supabase
    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024; // 5MB default
    public string[] AllowedExtensions { get; set; } = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    
    // Local storage settings
    public string LocalStoragePath { get; set; } = "wwwroot/uploads";
    public string BaseUrl { get; set; } = "/uploads";
    
    // Cloud storage settings (for future use)
    public string? CloudStorageUrl { get; set; }
    public string? CloudStorageKey { get; set; }
    public string? CloudStorageBucket { get; set; }
}
