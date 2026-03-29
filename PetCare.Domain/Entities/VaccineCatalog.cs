namespace PetCare.Domain.Entities;

using PetCare.Domain.Common;

public class VaccineCatalog : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Aliases { get; set; }
    public int? DefaultIntervalDays { get; set; }
    public bool IsActive { get; set; } = true;
}
