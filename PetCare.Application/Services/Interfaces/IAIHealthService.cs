using PetCare.Application.Common;
using PetCare.Domain.Entities;

namespace PetCare.Application.Services.Interfaces;

public interface IAIHealthService
{
    Task<ServiceResult<AIHealthAnalysis>> AnalyzePetHealthAsync(Guid petId, Guid userId, byte[] imageBytes);
    Task<ServiceResult<List<AIHealthAnalysis>>> GetPetAnalysisHistoryAsync(Guid petId, Guid userId);
    Task<ServiceResult<AIHealthAnalysis>> GetAnalysisByIdAsync(Guid analysisId, Guid userId);
    Task<ServiceResult<bool>> DeleteAnalysisAsync(Guid analysisId, Guid userId);
}
