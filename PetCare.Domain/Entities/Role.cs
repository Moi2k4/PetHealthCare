namespace PetCare.Domain.Entities;

using PetCare.Domain.Common;

public class Role : BaseEntity
{
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation properties
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
