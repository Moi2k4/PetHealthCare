namespace PetCare.Domain.Entities;

using PetCare.Domain.Common;

public class OrderStatusHistory : BaseEntity
{
    public Guid OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Navigation properties
    public virtual Order Order { get; set; } = null!;
    public virtual User? UpdatedByUser { get; set; }
}
