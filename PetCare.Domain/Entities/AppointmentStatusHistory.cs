namespace PetCare.Domain.Entities;

using PetCare.Domain.Common;

public class AppointmentStatusHistory : BaseEntity
{
    public Guid AppointmentId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Navigation properties
    public virtual Appointment Appointment { get; set; } = null!;
    public virtual User? UpdatedByUser { get; set; }
}
