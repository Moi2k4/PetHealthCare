namespace PetCare.Application.DTOs.Pet;

public class PetDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string PetName { get; set; } = string.Empty;
    public string? SpeciesName { get; set; }
    public string? BreedName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public decimal? Weight { get; set; }
    public string? Color { get; set; }
    public string? MicrochipId { get; set; }
    public string? AvatarUrl { get; set; }
    public string? SpecialNotes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
