namespace PetCare.Domain.Entities;

using PetCare.Domain.Common;

public class Product : AuditableEntity
{
    public Guid? CategoryId { get; set; }
    public Guid? BrandId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public int StockQuantity { get; set; } = 0;
    public string? Sku { get; set; }
    public decimal? Weight { get; set; }
    public string? Dimensions { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ProductCategory? Category { get; set; }
    public virtual Brand? Brand { get; set; }
    public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
}
