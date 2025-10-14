using System.ComponentModel.DataAnnotations;

namespace PetCare.Application.DTOs.User;

/// <summary>
/// DTO for users to update their own profile
/// </summary>
public class UpdateProfileDto
{
    [StringLength(100)]
    public string? FullName { get; set; }

    [Phone]
    [StringLength(20)]
    public string? Phone { get; set; }

    [StringLength(500)]
    public string? AvatarUrl { get; set; }

    [StringLength(200)]
    public string? Address { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(100)]
    public string? District { get; set; }
}
