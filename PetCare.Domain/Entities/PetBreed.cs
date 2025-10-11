namespace PetCare.Domain.Entities;

using PetCare.Domain.Common;

public class PetBreed : BaseEntity
{
    public Guid SpeciesId { get; set; }
    public string BreedName { get; set; } = string.Empty;
    public string? Characteristics { get; set; }

    // Navigation properties
    public virtual PetSpecies Species { get; set; } = null!;
    public virtual ICollection<Pet> Pets { get; set; } = new List<Pet>();
}
