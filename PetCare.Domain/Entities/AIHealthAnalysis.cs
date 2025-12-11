namespace PetCare.Domain.Entities;

using PetCare.Domain.Common;

public class AIHealthAnalysis : BaseEntity
{
    public Guid PetId { get; set; }
    public Guid UserId { get; set; }
    public string AnalysisType { get; set; } = string.Empty; // HealthProfile, Recommendation, DiseaseRisk, Nutrition
    public string InputData { get; set; } = string.Empty; // JSON of input parameters
    public string AIResponse { get; set; } = string.Empty; // AI-generated analysis
    public string? Recommendations { get; set; } // Specific recommendations
    public decimal? ConfidenceScore { get; set; }
    public int TokensUsed { get; set; } // Track AI API token usage
    public string? AIModel { get; set; } // GPT-4o, Gemini, etc.
    public bool IsReviewed { get; set; } = false; // Reviewed by vet/staff (as third party)
    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }

    // Navigation properties
    public virtual Pet Pet { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual User? Reviewer { get; set; }
}
