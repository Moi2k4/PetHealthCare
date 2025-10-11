namespace PetCare.Domain.Entities;

using PetCare.Domain.Common;

public class ChatMessage : BaseEntity
{
    public Guid SessionId { get; set; }
    public string SenderType { get; set; } = string.Empty; // "user" or "bot"
    public string MessageText { get; set; } = string.Empty;
    public string? MessageMetadata { get; set; } // JSON format

    // Navigation properties
    public virtual ChatSession Session { get; set; } = null!;
}
