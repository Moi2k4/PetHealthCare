namespace PetCare.Domain.Entities;

using PetCare.Domain.Common;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public bool IsRead { get; set; } = false;

    // Navigation properties
    public virtual User User { get; set; } = null!;
}
