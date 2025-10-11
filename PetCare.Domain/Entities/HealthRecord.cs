namespace PetCare.Domain.Entities;

using PetCare.Domain.Common;

public class HealthRecord : BaseEntity
{
    public Guid PetId { get; set; }
    public DateTime RecordDate { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Height { get; set; }
    public decimal? Temperature { get; set; }
    public int? HeartRate { get; set; }
    public string? Diagnosis { get; set; }
    public string? Treatment { get; set; }
    public string? Notes { get; set; }
    public Guid? RecordedBy { get; set; }

    // Navigation properties
    public virtual Pet Pet { get; set; } = null!;
    public virtual User? RecordedByUser { get; set; }
}
