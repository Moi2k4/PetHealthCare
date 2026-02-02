namespace PetCare.Domain.Entities;

using PetCare.Domain.Common;

public class Payment : AuditableEntity
{
    public Guid OrderId { get; set; }
    public Guid? UserId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty; // vnpay, momo, paypal, cod
    public string PaymentStatus { get; set; } = "pending"; // pending, processing, completed, failed, refunded
    public decimal Amount { get; set; }
    public string? TransactionId { get; set; }
    public string? PaymentGatewayResponse { get; set; } // JSON
    public DateTime? PaidAt { get; set; }
    public string? RefundReason { get; set; }
    public DateTime? RefundedAt { get; set; }
    public decimal? RefundAmount { get; set; }
    
    // Navigation properties
    public virtual Order Order { get; set; } = null!;
    public virtual User? User { get; set; }
}
