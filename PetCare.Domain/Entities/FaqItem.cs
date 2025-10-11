namespace PetCare.Domain.Entities;

using PetCare.Domain.Common;

public class FaqItem : AuditableEntity
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string[]? Keywords { get; set; }
    public int UsageCount { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}
