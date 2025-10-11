namespace PetCare.Domain.Entities;

using PetCare.Domain.Common;

public class Pet : AuditableEntity
{
    public Guid UserId { get; set; }
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

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual PetSpecies? Species { get; set; }
    public virtual PetBreed? Breed { get; set; }
    public virtual ICollection<HealthRecord> HealthRecords { get; set; } = new List<HealthRecord>();
    public virtual ICollection<Vaccination> Vaccinations { get; set; } = new List<Vaccination>();
    public virtual ICollection<HealthReminder> HealthReminders { get; set; } = new List<HealthReminder>();
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
