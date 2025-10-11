namespace PetCare.Domain.Entities;

using PetCare.Domain.Common;

public class Order : AuditableEntity
{
    public Guid UserId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string OrderStatus { get; set; } = "pending";
    public decimal TotalAmount { get; set; }
    public decimal ShippingFee { get; set; } = 0;
    public decimal DiscountAmount { get; set; } = 0;
    public decimal FinalAmount { get; set; }
    public string? PaymentMethod { get; set; }
    public string PaymentStatus { get; set; } = "unpaid";
    public string ShippingAddress { get; set; } = string.Empty;
    public string ShippingPhone { get; set; } = string.Empty;
    public string ShippingName { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime OrderedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
    public virtual ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
}
