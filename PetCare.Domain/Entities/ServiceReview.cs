namespace PetCare.Domain.Entities;

using PetCare.Domain.Common;

public class ServiceReview : BaseEntity
{
    public Guid AppointmentId { get; set; }
    public Guid? UserId { get; set; }
    public Guid? ServiceId { get; set; }
    public Guid? StaffId { get; set; }
    public int Rating { get; set; }
    public string? ReviewText { get; set; }
    public bool IsApproved { get; set; } = false;

    // Navigation properties
    public virtual Appointment Appointment { get; set; } = null!;
    public virtual User? User { get; set; }
    public virtual Service? Service { get; set; }
    public virtual User? Staff { get; set; }
}
