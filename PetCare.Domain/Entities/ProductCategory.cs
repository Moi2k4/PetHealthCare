namespace PetCare.Domain.Entities;

using PetCare.Domain.Common;

public class ProductCategory : AuditableEntity
{
    public string CategoryName { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ProductCategory? ParentCategory { get; set; }
    public virtual ICollection<ProductCategory> SubCategories { get; set; } = new List<ProductCategory>();
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
