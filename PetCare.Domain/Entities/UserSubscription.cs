namespace PetCare.Domain.Entities;

using PetCare.Domain.Common;

public class UserSubscription : AuditableEntity
{
    public Guid UserId { get; set; }
    public Guid SubscriptionPackageId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string Status { get; set; } = "Active"; // Active, Cancelled, Expired, Suspended
    public DateTime? NextBillingDate { get; set; }
    public decimal AmountPaid { get; set; }
    public string? PaymentMethod { get; set; }
    public string? TransactionId { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual SubscriptionPackage SubscriptionPackage { get; set; } = null!;
}
