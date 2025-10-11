namespace PetCare.Domain.Entities;

using PetCare.Domain.Common;

public class Service : BaseEntity
{
    public Guid? CategoryId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMinutes { get; set; }
    public decimal Price { get; set; }
    public bool IsHomeService { get; set; } = false;
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ServiceCategory? Category { get; set; }
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public virtual ICollection<StaffService> StaffServices { get; set; } = new List<StaffService>();
    public virtual ICollection<ServiceReview> Reviews { get; set; } = new List<ServiceReview>();
}
