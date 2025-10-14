namespace PetCare.Application.DTOs.Pet;

public class UpdatePetDto
{
    public Guid? SpeciesId { get; set; }
    public Guid? BreedId { get; set; }
    public string PetName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public decimal? Weight { get; set; }
    public string? Color { get; set; }
    public string? MicrochipId { get; set; }
    public string? AvatarUrl { get; set; }
    public string? SpecialNotes { get; set; }
    public bool IsActive { get; set; } = true;
}
