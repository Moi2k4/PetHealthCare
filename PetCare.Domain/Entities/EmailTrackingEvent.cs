using PetCare.Domain.Common;

namespace PetCare.Domain.Entities;

public class EmailTrackingEvent : BaseEntity
{
    public string EventType { get; set; } = string.Empty;
    public string? EmailId { get; set; }
    public string? Recipient { get; set; }
    public string? ClickedUrl { get; set; }
    public DateTime? EventTimestamp { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
}
