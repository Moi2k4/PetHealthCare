namespace PetCare.Domain.Entities;

using PetCare.Domain.Common;

public class SubscriptionPackage : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string BillingCycle { get; set; } = "Month"; // Month, Year
    public bool IsActive { get; set; } = true;
    
    // Features
    public bool HasAIHealthTracking { get; set; } = false;
    public bool HasVaccinationTracking { get; set; } = false;
    public bool HasHealthReminders { get; set; } = false;
    public bool HasAIRecommendations { get; set; } = false;
    public bool HasNutritionalAnalysis { get; set; } = false;
    public bool HasEarlyDiseaseDetection { get; set; } = false;
    public bool HasPrioritySupport { get; set; } = false;
    
    public int? MaxPets { get; set; } // null = unlimited
    public string? Features { get; set; } // JSON string of additional features

    // Navigation properties
    public virtual ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
}
