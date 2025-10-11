namespace PetCare.Domain.Entities;

using PetCare.Domain.Common;

public class Vaccination : BaseEntity
{
    public Guid PetId { get; set; }
    public string VaccineName { get; set; } = string.Empty;
    public DateTime VaccinationDate { get; set; }
    public DateTime? NextDueDate { get; set; }
    public string? BatchNumber { get; set; }
    public Guid? AdministeredBy { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Pet Pet { get; set; } = null!;
    public virtual User? AdministeredByUser { get; set; }
}
