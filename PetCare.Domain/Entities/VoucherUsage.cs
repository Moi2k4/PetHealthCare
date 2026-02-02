namespace PetCare.Domain.Entities;

using PetCare.Domain.Common;

public class VoucherUsage : BaseEntity
{
    public Guid VoucherId { get; set; }
    public Guid UserId { get; set; }
    public Guid? OrderId { get; set; }
    public decimal DiscountAmount { get; set; }
    public DateTime UsedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Voucher Voucher { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual Order? Order { get; set; }
}
