namespace PetCare.Application.DTOs.User;

public class CreateUserDto
{
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public Guid? RoleId { get; set; }
    public string Password { get; set; } = string.Empty;
}
