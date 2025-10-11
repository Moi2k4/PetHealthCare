namespace PetCare.Domain.Entities;

using PetCare.Domain.Common;

public class StaffSchedule : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid? BranchId { get; set; }
    public DateTime WorkDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsAvailable { get; set; } = true;

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Branch? Branch { get; set; }
}
