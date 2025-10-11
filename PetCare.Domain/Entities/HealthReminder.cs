namespace PetCare.Domain.Entities;

using PetCare.Domain.Common;

public class HealthReminder : BaseEntity
{
    public Guid PetId { get; set; }
    public string ReminderType { get; set; } = string.Empty;
    public string ReminderTitle { get; set; } = string.Empty;
    public DateTime ReminderDate { get; set; }
    public bool IsCompleted { get; set; } = false;
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Pet Pet { get; set; } = null!;
}
