namespace PetCare.Domain.Entities;

using PetCare.Domain.Common;

public class Brand : BaseEntity
{
    public string BrandName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }

    // Navigation properties
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
