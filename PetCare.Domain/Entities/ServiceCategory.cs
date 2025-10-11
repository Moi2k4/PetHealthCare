namespace PetCare.Domain.Entities;

using PetCare.Domain.Common;

public class ServiceCategory : BaseEntity
{
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }

    // Navigation properties
    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
}
