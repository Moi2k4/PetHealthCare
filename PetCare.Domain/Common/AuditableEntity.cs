namespace PetCare.Domain.Common;

/// <summary>
/// Base entity with audit fields
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    public DateTime? UpdatedAt { get; set; }
}
