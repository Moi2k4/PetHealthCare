namespace PetCare.Domain.Entities;

using PetCare.Domain.Common;

public class CartItem : AuditableEntity
{
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; } = 1;

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}
