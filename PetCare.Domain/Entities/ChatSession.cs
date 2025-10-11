namespace PetCare.Domain.Entities;

using PetCare.Domain.Common;

public class ChatSession : BaseEntity
{
    public Guid? UserId { get; set; }
    public DateTime SessionStart { get; set; } = DateTime.UtcNow;
    public DateTime? SessionEnd { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual User? User { get; set; }
    public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
