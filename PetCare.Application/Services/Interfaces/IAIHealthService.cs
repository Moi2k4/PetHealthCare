using PetCare.Application.Common;
using PetCare.Application.DTOs.Health;

namespace PetCare.Application.Services.Interfaces;

public interface IAIHealthService
{
    /// <summary>
    /// Analyse a pet's health records using Google Gemini and return an AI-generated report.
    /// </summary>
    Task<ServiceResult<AIHealthAnalysisResponseDto>> AnalyseHealthAsync(
        AIHealthAnalysisRequestDto request, Guid requestingUserId);

    /// <summary>
    /// List all AI analysis records for a pet (most recent first).
    /// </summary>
    Task<ServiceResult<IEnumerable<AIHealthAnalysisSummaryDto>>> GetAnalysisHistoryAsync(
        Guid petId, Guid requestingUserId);

    /// <summary>
    /// Get a single AI analysis record by its ID.
    /// </summary>
    Task<ServiceResult<AIHealthAnalysisResponseDto>> GetAnalysisByIdAsync(
        Guid analysisId, Guid requestingUserId);
}
