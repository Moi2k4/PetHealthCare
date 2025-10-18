namespace PetCare.Application.DTOs.Pet;

/// <summary>
/// DTO for Pet Species
/// </summary>
public class PetSpeciesDto
{
    public Guid Id { get; set; }
    public string SpeciesName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for Pet Breed
/// </summary>
public class PetBreedDto
{
    public Guid Id { get; set; }
    public Guid SpeciesId { get; set; }
    public string BreedName { get; set; } = string.Empty;
    public string? Characteristics { get; set; }
    public string? SpeciesName { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for Species with its Breeds
/// </summary>
public class SpeciesWithBreedsDto
{
    public Guid Id { get; set; }
    public string SpeciesName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<PetBreedDto> Breeds { get; set; } = new();
}
