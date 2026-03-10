namespace PetCare.Application.DTOs.Health;

public class AIHealthAnalysisRequestDto
{
    public Guid PetId { get; set; }

    /// <summary>
    /// Type of analysis: HealthProfile, Recommendation, DiseaseRisk, Nutrition
    /// </summary>
    public string AnalysisType { get; set; } = "HealthProfile";

    /// <summary>
    /// Optional additional question or context the owner wants analysed
    /// </summary>
    public string? AdditionalContext { get; set; }
}

public class AIHealthAnalysisResponseDto
{
    public Guid Id { get; set; }
    public Guid PetId { get; set; }
    public string PetName { get; set; } = string.Empty;
    public string AnalysisType { get; set; } = string.Empty;
    public string AIResponse { get; set; } = string.Empty;
    public string? Recommendations { get; set; }
    public decimal? ConfidenceScore { get; set; }
    public string AIModel { get; set; } = string.Empty;
    public bool IsReviewed { get; set; }
    public string? ReviewNotes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AIHealthAnalysisSummaryDto
{
    public Guid Id { get; set; }
    public string AnalysisType { get; set; } = string.Empty;
    public string AIModel { get; set; } = string.Empty;
    public bool IsReviewed { get; set; }
    public DateTime CreatedAt { get; set; }
}
