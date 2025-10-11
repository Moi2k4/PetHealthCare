namespace PetCare.Domain.Entities;

using PetCare.Domain.Common;

public class Appointment : AuditableEntity
{
    public Guid UserId { get; set; }
    public Guid? PetId { get; set; }
    public Guid? ServiceId { get; set; }
    public string AppointmentType { get; set; } = string.Empty;
    public string AppointmentStatus { get; set; } = "pending";
    public Guid? BranchId { get; set; }
    public Guid? AssignedStaffId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string? ServiceAddress { get; set; }
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Pet? Pet { get; set; }
    public virtual Service? Service { get; set; }
    public virtual Branch? Branch { get; set; }
    public virtual User? AssignedStaff { get; set; }
    public virtual ICollection<AppointmentStatusHistory> StatusHistory { get; set; } = new List<AppointmentStatusHistory>();
    public virtual ICollection<ServiceReview> Reviews { get; set; } = new List<ServiceReview>();
}
