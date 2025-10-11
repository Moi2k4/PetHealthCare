namespace PetCare.Domain.Entities;

using PetCare.Domain.Common;

public class ProductImage : BaseEntity
{
    public Guid ProductId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int DisplayOrder { get; set; } = 0;
    public bool IsPrimary { get; set; } = false;

    // Navigation properties
    public virtual Product Product { get; set; } = null!;
}
