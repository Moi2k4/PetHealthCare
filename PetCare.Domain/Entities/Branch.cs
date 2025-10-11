namespace PetCare.Domain.Entities;

using PetCare.Domain.Common;

public class Branch : BaseEntity
{
    public string BranchName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? OpeningHours { get; set; } // JSON format
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public virtual ICollection<StaffSchedule> StaffSchedules { get; set; } = new List<StaffSchedule>();
}
