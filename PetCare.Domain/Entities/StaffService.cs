namespace PetCare.Domain.Entities;

using PetCare.Domain.Common;

public class StaffService : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid ServiceId { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Service Service { get; set; } = null!;
}
