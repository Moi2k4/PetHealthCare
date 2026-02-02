namespace PetCare.Domain.Entities;

using PetCare.Domain.Common;

public class Voucher : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DiscountType { get; set; } = "percentage"; // percentage, fixed
    public decimal DiscountValue { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public decimal? MaximumDiscountAmount { get; set; }
    public int? UsageLimit { get; set; }
    public int UsedCount { get; set; } = 0;
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public bool IsActive { get; set; } = true;
    public string? ApplicableProductCategories { get; set; } // JSON array
    public string? ApplicableServices { get; set; } // JSON array
    
    // Navigation properties
    public virtual ICollection<VoucherUsage> VoucherUsages { get; set; } = new List<VoucherUsage>();
}
