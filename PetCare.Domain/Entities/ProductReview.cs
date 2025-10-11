namespace PetCare.Domain.Entities;

using PetCare.Domain.Common;

public class ProductReview : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid? UserId { get; set; }
    public Guid? OrderId { get; set; }
    public int Rating { get; set; }
    public string? ReviewText { get; set; }
    public string[]? Images { get; set; }
    public bool IsVerifiedPurchase { get; set; } = false;
    public bool IsApproved { get; set; } = false;

    // Navigation properties
    public virtual Product Product { get; set; } = null!;
    public virtual User? User { get; set; }
    public virtual Order? Order { get; set; }
}
