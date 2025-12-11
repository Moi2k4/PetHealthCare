using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PetCare.Application.Common;
using PetCare.Application.Services.Interfaces;
using PetCare.Domain.Entities;
using PetCare.Infrastructure.Data;

namespace PetCare.Application.Services.Implementations;

public class AIHealthService : IAIHealthService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly PetCareDbContext _context;
    private readonly ILogger<AIHealthService> _logger;

    public AIHealthService(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        PetCareDbContext context,
        ILogger<AIHealthService> logger)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _context = context;
        _logger = logger;
    }

    public async Task<ServiceResult<AIHealthAnalysis>> CreateHealthProfileAsync(Guid petId, Guid userId)
    {
        try
        {
            // Check if user has premium subscription
            var hasSubscription = await CheckPremiumSubscriptionAsync(userId);
            if (!hasSubscription)
            {
                return ServiceResult<AIHealthAnalysis>.FailureResult("Premium Health Tracking Package required for AI health profiles.");
            }

            // Get pet data
            var pet = await _context.Pets
                .Include(p => p.Species)
                .Include(p => p.Breed)
                .Include(p => p.HealthRecords)
                .Include(p => p.Vaccinations)
                .FirstOrDefaultAsync(p => p.Id == petId && p.UserId == userId);

            if (pet == null)
            {
                return ServiceResult<AIHealthAnalysis>.FailureResult("Pet not found.");
            }

            // Build AI prompt
            var inputData = BuildHealthProfileInput(pet);
            var aiResponse = await CallAIApiAsync("health_profile", inputData);

            // Save analysis
            var analysis = new AIHealthAnalysis
            {
                Id = Guid.NewGuid(),
                PetId = petId,
                UserId = userId,
                AnalysisType = "HealthProfile",
                InputData = JsonSerializer.Serialize(inputData),
                AIResponse = aiResponse.Response,
                TokensUsed = aiResponse.TokensUsed,
                AIModel = aiResponse.Model,
                ConfidenceScore = aiResponse.ConfidenceScore,
                CreatedAt = DateTime.UtcNow
            };

            await _context.AIHealthAnalyses.AddAsync(analysis);
            await _context.SaveChangesAsync();

            return ServiceResult<AIHealthAnalysis>.SuccessResult(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating AI health profile for pet {PetId}", petId);
            return ServiceResult<AIHealthAnalysis>.FailureResult("Failed to create health profile. Please try again.");
        }
    }

    public async Task<ServiceResult<AIHealthAnalysis>> GetPersonalizedRecommendationsAsync(Guid petId, Guid userId)
    {
        try
        {
            var hasSubscription = await CheckPremiumSubscriptionAsync(userId);
            if (!hasSubscription)
            {
                return ServiceResult<AIHealthAnalysis>.FailureResult("Premium subscription required for personalized recommendations.");
            }

            var pet = await _context.Pets
                .Include(p => p.Species)
                .Include(p => p.Breed)
                .Include(p => p.HealthRecords)
                .Include(p => p.Vaccinations)
                .FirstOrDefaultAsync(p => p.Id == petId && p.UserId == userId);

            if (pet == null)
            {
                return ServiceResult<AIHealthAnalysis>.FailureResult("Pet not found or you don't have permission to access it.");
            }

            var inputData = BuildRecommendationInput(pet);
            var aiResponse = await CallAIApiAsync("recommendations", inputData);

            var analysis = new AIHealthAnalysis
            {
                Id = Guid.NewGuid(),
                PetId = petId,
                UserId = userId,
                AnalysisType = "Recommendation",
                InputData = JsonSerializer.Serialize(inputData),
                AIResponse = aiResponse.Response,
                Recommendations = aiResponse.Response,
                TokensUsed = aiResponse.TokensUsed,
                AIModel = aiResponse.Model,
                CreatedAt = DateTime.UtcNow
            };

            await _context.AIHealthAnalyses.AddAsync(analysis);
            await _context.SaveChangesAsync();

            return ServiceResult<AIHealthAnalysis>.SuccessResult(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting personalized recommendations for pet {PetId}", petId);
            return ServiceResult<AIHealthAnalysis>.FailureResult("Failed to generate recommendations. Please try again.");
        }
    }

    public async Task<ServiceResult<AIHealthAnalysis>> AnalyzeNutritionAsync(Guid petId, Guid userId, string dietInfo)
    {
        try
        {
            var hasSubscription = await CheckPremiumSubscriptionAsync(userId);
            if (!hasSubscription)
            {
                return ServiceResult<AIHealthAnalysis>.FailureResult("Premium subscription required for nutrition analysis.");
            }

            var pet = await _context.Pets
                .Include(p => p.Species)
                .Include(p => p.Breed)
                .FirstOrDefaultAsync(p => p.Id == petId && p.UserId == userId);

            if (pet == null)
            {
                return ServiceResult<AIHealthAnalysis>.FailureResult("Pet not found or you don't have permission to access it.");
            }

            var inputData = new
            {
                pet_name = pet.PetName,
                species = pet.Species?.SpeciesName,
                breed = pet.Breed?.BreedName,
                age_years = pet.DateOfBirth.HasValue ? CalculateAge(pet.DateOfBirth.Value) : 0,
                weight = pet.Weight,
                current_diet = dietInfo
            };

            var aiResponse = await CallAIApiAsync("nutrition_analysis", inputData);

            var analysis = new AIHealthAnalysis
            {
                Id = Guid.NewGuid(),
                PetId = petId,
                UserId = userId,
                AnalysisType = "Nutrition",
                InputData = JsonSerializer.Serialize(inputData),
                AIResponse = aiResponse.Response,
                Recommendations = aiResponse.Response,
                TokensUsed = aiResponse.TokensUsed,
                AIModel = aiResponse.Model,
                CreatedAt = DateTime.UtcNow
            };

            await _context.AIHealthAnalyses.AddAsync(analysis);
            await _context.SaveChangesAsync();

            return ServiceResult<AIHealthAnalysis>.SuccessResult(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing nutrition for pet {PetId}", petId);
            return ServiceResult<AIHealthAnalysis>.FailureResult("Failed to analyze nutrition. Please try again.");
        }
    }

    public async Task<ServiceResult<AIHealthAnalysis>> DetectDiseaseRiskAsync(Guid petId, Guid userId, string symptoms)
    {
        try
        {
            var hasSubscription = await CheckPremiumSubscriptionAsync(userId);
            if (!hasSubscription)
            {
                return ServiceResult<AIHealthAnalysis>.FailureResult("Premium subscription required for disease risk detection.");
            }

            var pet = await _context.Pets
                .Include(p => p.Species)
                .Include(p => p.Breed)
                .Include(p => p.HealthRecords)
                .FirstOrDefaultAsync(p => p.Id == petId && p.UserId == userId);

            if (pet == null)
            {
                return ServiceResult<AIHealthAnalysis>.FailureResult("Pet not found or you don't have permission to access it.");
            }

            var inputData = new
            {
                pet_name = pet.PetName,
                species = pet.Species?.SpeciesName,
                breed = pet.Breed?.BreedName,
                age_years = pet.DateOfBirth.HasValue ? CalculateAge(pet.DateOfBirth.Value) : 0,
                weight = pet.Weight,
                symptoms = symptoms,
                health_history = pet.HealthRecords.OrderByDescending(h => h.RecordDate).Take(5).Select(h => new
                {
                    date = h.RecordDate,
                    diagnosis = h.Diagnosis,
                    treatment = h.Treatment
                })
            };

            var prompt = $@"As a pet health analysis assistant (NOT a veterinarian), analyze the following symptoms for a {pet.Species?.SpeciesName} named {pet.PetName}.

IMPORTANT: You are a third-party pet care platform, NOT a veterinary clinic. Provide general guidance and recommendations to seek professional veterinary care when needed.

Symptoms: {symptoms}

Provide:
1. Possible causes (general information only)
2. Severity assessment (Low/Medium/High risk)
3. Recommended actions (emphasize consulting a licensed veterinarian)
4. Preventive measures
5. When to seek immediate veterinary attention

DISCLAIMER: This is AI-generated information for educational purposes only and should NOT replace professional veterinary advice.";

            var aiResponse = await CallAIApiAsync("disease_detection", inputData, prompt);

            var analysis = new AIHealthAnalysis
            {
                Id = Guid.NewGuid(),
                PetId = petId,
                UserId = userId,
                AnalysisType = "DiseaseRisk",
                InputData = JsonSerializer.Serialize(inputData),
                AIResponse = aiResponse.Response,
                Recommendations = aiResponse.Response,
                TokensUsed = aiResponse.TokensUsed,
                AIModel = aiResponse.Model,
                ConfidenceScore = aiResponse.ConfidenceScore,
                IsReviewed = false, // Third-party staff can review later
                CreatedAt = DateTime.UtcNow
            };

            await _context.AIHealthAnalyses.AddAsync(analysis);
            await _context.SaveChangesAsync();

            return ServiceResult<AIHealthAnalysis>.SuccessResult(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting disease risk for pet {PetId}", petId);
            return ServiceResult<AIHealthAnalysis>.FailureResult("Failed to detect disease risks. Please try again.");
        }
    }

    public async Task<ServiceResult<List<AIHealthAnalysis>>> GetPetHealthHistoryAsync(Guid petId)
    {
        try
        {
            var analyses = await _context.AIHealthAnalyses
                .Where(a => a.PetId == petId)
                .OrderByDescending(a => a.CreatedAt)
                .Take(20)
                .ToListAsync();

            return ServiceResult<List<AIHealthAnalysis>>.SuccessResult(analyses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting health history for pet {PetId}", petId);
            return ServiceResult<List<AIHealthAnalysis>>.FailureResult("Failed to retrieve health history.");
        }
    }

    // Helper methods
    private async Task<bool> CheckPremiumSubscriptionAsync(Guid userId)
    {
        return await _context.UserSubscriptions
            .AnyAsync(s => s.UserId == userId 
                && s.IsActive 
                && s.Status == "Active"
                && (s.EndDate == null || s.EndDate > DateTime.UtcNow));
    }

    private object BuildHealthProfileInput(Pet pet)
    {
        return new
        {
            pet_name = pet.PetName,
            species = pet.Species?.SpeciesName,
            breed = pet.Breed?.BreedName,
            breed_characteristics = pet.Breed?.Characteristics,
            age_years = pet.DateOfBirth.HasValue ? CalculateAge(pet.DateOfBirth.Value) : 0,
            gender = pet.Gender,
            weight = pet.Weight,
            vaccinations = pet.Vaccinations.Select(v => new
            {
                vaccine = v.VaccineName,
                date = v.VaccinationDate,
                next_due = v.NextDueDate
            }),
            recent_health_records = pet.HealthRecords.OrderByDescending(h => h.RecordDate).Take(3).Select(h => new
            {
                date = h.RecordDate,
                weight = h.Weight,
                diagnosis = h.Diagnosis,
                treatment = h.Treatment
            })
        };
    }

    private object BuildRecommendationInput(Pet pet)
    {
        return new
        {
            pet_name = pet.PetName,
            species = pet.Species?.SpeciesName,
            breed = pet.Breed?.BreedName,
            age_years = pet.DateOfBirth.HasValue ? CalculateAge(pet.DateOfBirth.Value) : 0,
            weight = pet.Weight,
            special_notes = pet.SpecialNotes,
            vaccinations_up_to_date = pet.Vaccinations.Any(v => v.NextDueDate > DateTime.UtcNow)
        };
    }

    private async Task<(string Response, int TokensUsed, string Model, decimal? ConfidenceScore)> CallAIApiAsync(
        string analysisType, 
        object inputData, 
        string? customPrompt = null)
    {
        // Use Google Gemini for cost efficiency (cheaper than OpenAI)
        var apiKey = _configuration["GEMINI_API_KEY"];
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new Exception("AI API key not configured");
        }

        var client = _httpClientFactory.CreateClient();
        
        var prompt = customPrompt ?? BuildDefaultPrompt(analysisType, inputData);
        
        // Simulate AI API call (replace with actual Gemini/OpenAI API call)
        var response = $"AI Analysis for {analysisType}: Based on the provided data, here are the recommendations...";
        var tokensUsed = EstimateTokens(prompt + response);

        return (response, tokensUsed, "gemini-1.5-flash", 0.85m);
    }

    private string BuildDefaultPrompt(string analysisType, object inputData)
    {
        var dataJson = JsonSerializer.Serialize(inputData);
        
        return analysisType switch
        {
            "health_profile" => $"Create a comprehensive health profile for this pet. Data: {dataJson}",
            "recommendations" => $"Provide personalized care recommendations. Data: {dataJson}",
            "nutrition_analysis" => $"Analyze nutritional needs and diet. Data: {dataJson}",
            _ => $"Analyze this pet health data: {dataJson}"
        };
    }

    private int CalculateAge(DateTime dateOfBirth)
    {
        var today = DateTime.UtcNow;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age)) age--;
        return age;
    }

    private int EstimateTokens(string text)
    {
        // Rough estimation: 1 token ≈ 4 characters
        return text.Length / 4;
    }

    // Methods for controller interface
    public async Task<ServiceResult<AIHealthAnalysis>> AnalyzePetHealthAsync(Guid petId, Guid userId, byte[] imageBytes)
    {
        try
        {
            var pet = await _context.Pets.FirstOrDefaultAsync(p => p.Id == petId && p.UserId == userId);
            if (pet == null)
                return ServiceResult<AIHealthAnalysis>.FailureResult("Pet not found or you don't have permission");

            // Create analysis record
            var analysis = new AIHealthAnalysis
            {
                PetId = petId,
                AnalysisType = "ImageAnalysis",
                InputData = Convert.ToBase64String(imageBytes),
                AIResponse = "Image analysis placeholder - integrate with AI service",
                ConfidenceScore = 0.85m,
                CreatedAt = DateTime.UtcNow
            };

            _context.AIHealthAnalyses.Add(analysis);
            await _context.SaveChangesAsync();

            return ServiceResult<AIHealthAnalysis>.SuccessResult(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing pet health for pet: {PetId}", petId);
            return ServiceResult<AIHealthAnalysis>.FailureResult($"Error analyzing health: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<AIHealthAnalysis>>> GetPetAnalysisHistoryAsync(Guid petId, Guid userId)
    {
        try
        {
            var pet = await _context.Pets.FirstOrDefaultAsync(p => p.Id == petId && p.UserId == userId);
            if (pet == null)
                return ServiceResult<List<AIHealthAnalysis>>.FailureResult("Pet not found or you don't have permission");

            var analyses = await _context.AIHealthAnalyses
                .Where(a => a.PetId == petId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return ServiceResult<List<AIHealthAnalysis>>.SuccessResult(analyses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting analysis history for pet: {PetId}", petId);
            return ServiceResult<List<AIHealthAnalysis>>.FailureResult($"Error retrieving analysis history: {ex.Message}");
        }
    }

    public async Task<ServiceResult<AIHealthAnalysis>> GetAnalysisByIdAsync(Guid analysisId, Guid userId)
    {
        try
        {
            var analysis = await _context.AIHealthAnalyses
                .Include(a => a.Pet)
                .FirstOrDefaultAsync(a => a.Id == analysisId);

            if (analysis == null)
                return ServiceResult<AIHealthAnalysis>.FailureResult("Analysis not found");

            if (analysis.Pet.UserId != userId)
                return ServiceResult<AIHealthAnalysis>.FailureResult("You don't have permission to view this analysis");

            return ServiceResult<AIHealthAnalysis>.SuccessResult(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting analysis: {AnalysisId}", analysisId);
            return ServiceResult<AIHealthAnalysis>.FailureResult($"Error retrieving analysis: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> DeleteAnalysisAsync(Guid analysisId, Guid userId)
    {
        try
        {
            var analysis = await _context.AIHealthAnalyses
                .Include(a => a.Pet)
                .FirstOrDefaultAsync(a => a.Id == analysisId);

            if (analysis == null)
                return ServiceResult<bool>.FailureResult("Analysis not found");

            if (analysis.Pet.UserId != userId)
                return ServiceResult<bool>.FailureResult("You don't have permission to delete this analysis");

            _context.AIHealthAnalyses.Remove(analysis);
            await _context.SaveChangesAsync();

            return ServiceResult<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting analysis: {AnalysisId}", analysisId);
            return ServiceResult<bool>.FailureResult($"Error deleting analysis: {ex.Message}");
        }
    }
}


