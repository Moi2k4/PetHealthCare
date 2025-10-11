namespace PetCare.Domain.Entities;

using PetCare.Domain.Common;

public class PetSpecies : BaseEntity
{
    public string SpeciesName { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation properties
    public virtual ICollection<PetBreed> Breeds { get; set; } = new List<PetBreed>();
    public virtual ICollection<Pet> Pets { get; set; } = new List<Pet>();
}
