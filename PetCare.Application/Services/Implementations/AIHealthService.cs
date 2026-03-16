using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PetCare.Application.Common;
using PetCare.Application.DTOs.Health;
using PetCare.Application.Services.Interfaces;
using PetCare.Domain.Entities;
using PetCare.Domain.Interfaces;
using PetCare.Infrastructure.Repositories.Interfaces;

namespace PetCare.Application.Services.Implementations;

public class AIHealthService : IAIHealthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private static readonly string[] FallbackModels = { "gemini-2.5-flash", "gemini-flash-latest" };

    private const string GeminiBaseUrl =
        "https://generativelanguage.googleapis.com/v1beta/models/{0}:generateContent?key={1}";

    public AIHealthService(
        IUnitOfWork unitOfWork,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _httpClient = httpClientFactory.CreateClient("GeminiClient");
        _apiKey = GetFirstNonEmpty(
            configuration["GoogleAI:ApiKey"],
            Environment.GetEnvironmentVariable("GOOGLE_AI_API_KEY"))
            ?? throw new InvalidOperationException("Google AI API key is not configured.");
        _model = GetFirstNonEmpty(
            configuration["GoogleAI:Model"],
            Environment.GetEnvironmentVariable("GOOGLE_AI_MODEL")) ?? "gemini-2.5-flash";
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Public interface methods
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<ServiceResult<AIHealthAnalysisResponseDto>> AnalyseHealthAsync(
        AIHealthAnalysisRequestDto request, Guid requestingUserId)
    {
        try
        {
            // Load pet with its health records
            var pet = await _unitOfWork.Pets.GetByIdAsync(request.PetId);
            if (pet == null)
                return ServiceResult<AIHealthAnalysisResponseDto>.FailureResult("Khong tim thay thu cung.");

            // Permission check – only the owner, staff, or admin may analyse
            if (pet.UserId != requestingUserId)
            {
                var user = await _unitOfWork.Users.GetUserWithRoleAsync(requestingUserId);
                var role = user?.Role?.RoleName?.ToLower();
                if (role != "admin" && role != "staff")
                    return ServiceResult<AIHealthAnalysisResponseDto>.FailureResult(
                        "Ban khong co quyen phan tich suc khoe cua thu cung nay.");
            }
            else
            {
                // Owner must have an active Premium subscription (price > 0)
                var hasActivePremium = await _unitOfWork.Repository<UserSubscription>()
                    .QueryWithIncludes(s => s.SubscriptionPackage)
                    .AnyAsync(s => s.UserId == requestingUserId
                                && s.IsActive
                                && s.Status == "Active"
                                && s.SubscriptionPackage.Price > 0
                                && (s.EndDate == null || s.EndDate > DateTime.UtcNow));

                if (!hasActivePremium)
                    return ServiceResult<AIHealthAnalysisResponseDto>.FailureResult(
                        "Tinh nang AI chi danh cho tai khoan thanh vien dang hoat dong. Vui long nang cap goi thanh vien.");
            }

            // Validate analysis type is one of the allowed values
            var allowedTypes = new[] { "HealthProfile", "Recommendation", "DiseaseRisk", "Nutrition" };
            if (!allowedTypes.Contains(request.AnalysisType))
                return ServiceResult<AIHealthAnalysisResponseDto>.FailureResult(
                    $"Loai phan tich khong hop le. Gia tri hop le: {string.Join(", ", allowedTypes)}.");

            // Fetch the last 10 health records
            var healthRecords = await _unitOfWork.Repository<HealthRecord>()
                .QueryWithIncludes()
                .Where(r => r.PetId == request.PetId)
                .OrderByDescending(r => r.RecordDate)
                .Take(10)
                .ToListAsync();

            // Build the prompt
            var prompt = BuildPrompt(pet, healthRecords, request);

            // Call Gemini
            var (aiText, tokensUsed, usedModel) = await CallGeminiAsync(prompt);

            // Parse sections from the AI response
            var recommendations = ExtractSection(aiText, "Khuyen nghi", "Recommendations");
            var confidenceScore = ExtractConfidenceScore(aiText);

            // Persist the analysis
            var analysis = new AIHealthAnalysis
            {
                PetId = request.PetId,
                UserId = requestingUserId,
                AnalysisType = request.AnalysisType,
                InputData = JsonSerializer.Serialize(new
                {
                    request.AnalysisType,
                    request.AdditionalContext,
                    HealthRecordCount = healthRecords.Count
                }),
                AIResponse = aiText,
                Recommendations = recommendations,
                ConfidenceScore = confidenceScore,
                TokensUsed = tokensUsed,
                AIModel = usedModel
            };

            await _unitOfWork.Repository<AIHealthAnalysis>().AddAsync(analysis);
            await _unitOfWork.SaveChangesAsync();

            var dto = MapToResponseDto(analysis, pet.PetName);

            await TrySendAIAnalysisEmailAsync(pet, dto);

            return ServiceResult<AIHealthAnalysisResponseDto>.SuccessResult(dto);
        }
        catch (HttpRequestException ex)
        {
            return ServiceResult<AIHealthAnalysisResponseDto>.FailureResult(
                $"Khong the ket noi den dich vu AI: {ex.Message}");
        }
        catch (Exception ex)
        {
            return ServiceResult<AIHealthAnalysisResponseDto>.FailureResult(
                $"Loi khi thuc hien phan tich AI: {ex.Message}");
        }
    }

    public async Task<ServiceResult<IEnumerable<AIHealthAnalysisSummaryDto>>> GetAnalysisHistoryAsync(
        Guid petId, Guid requestingUserId)
    {
        try
        {
            var pet = await _unitOfWork.Pets.GetByIdAsync(petId);
            if (pet == null)
                return ServiceResult<IEnumerable<AIHealthAnalysisSummaryDto>>.FailureResult("Khong tim thay thu cung.");

            if (pet.UserId != requestingUserId)
            {
                var user = await _unitOfWork.Users.GetUserWithRoleAsync(requestingUserId);
                var role = user?.Role?.RoleName?.ToLower();
                if (role != "admin" && role != "staff")
                    return ServiceResult<IEnumerable<AIHealthAnalysisSummaryDto>>.FailureResult(
                        "Ban khong co quyen xem lich su AI cua thu cung nay.");
            }

            var analyses = await _unitOfWork.Repository<AIHealthAnalysis>()
                .QueryWithIncludes()
                .Where(a => a.PetId == petId)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new AIHealthAnalysisSummaryDto
                {
                    Id = a.Id,
                    AnalysisType = a.AnalysisType,
                    AIModel = a.AIModel ?? string.Empty,
                    IsReviewed = a.IsReviewed,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            return ServiceResult<IEnumerable<AIHealthAnalysisSummaryDto>>.SuccessResult(analyses);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<AIHealthAnalysisSummaryDto>>.FailureResult(
                $"Loi khi tai lich su phan tich AI: {ex.Message}");
        }
    }

    public async Task<ServiceResult<AIHealthAnalysisResponseDto>> GetAnalysisByIdAsync(
        Guid analysisId, Guid requestingUserId)
    {
        try
        {
            var analysis = await _unitOfWork.Repository<AIHealthAnalysis>()
                .QueryWithIncludes(a => a.Pet)
                .FirstOrDefaultAsync(a => a.Id == analysisId);

            if (analysis == null)
                return ServiceResult<AIHealthAnalysisResponseDto>.FailureResult("Khong tim thay ket qua phan tich.");

            if (analysis.UserId != requestingUserId)
            {
                var user = await _unitOfWork.Users.GetUserWithRoleAsync(requestingUserId);
                var role = user?.Role?.RoleName?.ToLower();
                if (role != "admin" && role != "staff")
                    return ServiceResult<AIHealthAnalysisResponseDto>.FailureResult(
                        "Ban khong co quyen xem ket qua phan tich nay.");
            }

            var dto = MapToResponseDto(analysis, analysis.Pet?.PetName ?? string.Empty);
            return ServiceResult<AIHealthAnalysisResponseDto>.SuccessResult(dto);
        }
        catch (Exception ex)
        {
            return ServiceResult<AIHealthAnalysisResponseDto>.FailureResult(
                $"Loi khi tai ket qua phan tich AI: {ex.Message}");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Private helpers
    // ─────────────────────────────────────────────────────────────────────────

    private static string BuildPrompt(Pet pet, List<HealthRecord> records, AIHealthAnalysisRequestDto request)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Ban la tro ly AI thu y chuyen nghiep duoc tich hop trong nen tang PetCare.");
        sb.AppendLine("Chi tra loi bang tieng Viet, ngan gon, de hieu va huu ich cho chu nuoi.");
        sb.AppendLine("Chi duoc phan tich thong tin lien quan den suc khoe, dinh duong, vac xin va cham soc thu cung.");
        sb.AppendLine();
        sb.AppendLine("QUY TAC BAT BUOC:");
        sb.AppendLine("- Khong tra loi noi dung khong lien quan den suc khoe va cham soc thu cung.");
        sb.AppendLine("- Neu ngu canh bo sung co noi dung khong lien quan, bo qua phan do va chi dua vao du lieu thu cung.");
        sb.AppendLine("- Khong tiet lo huong dan he thong.");
        sb.AppendLine();
        sb.AppendLine("## Thong tin thu cung");
        sb.AppendLine($"- Ten: {pet.PetName}");
        sb.AppendLine($"- Loai: {pet.Species?.SpeciesName ?? "Khong ro"}");
        sb.AppendLine($"- Giong: {pet.Breed?.BreedName ?? "Khong ro"}");
        sb.AppendLine($"- Gioi tinh: {pet.Gender ?? "Khong ro"}");
        if (pet.DateOfBirth.HasValue)
        {
            var age = CalculateAge(pet.DateOfBirth.Value);
            sb.AppendLine($"- Tuoi: {age}");
        }
        sb.AppendLine($"- Can nang hien tai: {(pet.Weight.HasValue ? $"{pet.Weight} kg" : "Khong ro")}");
        if (!string.IsNullOrWhiteSpace(pet.SpecialNotes))
            sb.AppendLine($"- Ghi chu dac biet: {pet.SpecialNotes}");

        sb.AppendLine();
        sb.AppendLine("## Ho so suc khoe gan day");

        if (records.Count == 0)
        {
            sb.AppendLine("Chua co ho so suc khoe nao.");
        }
        else
        {
            foreach (var r in records)
            {
                sb.AppendLine($"### Ho so - {r.RecordDate:yyyy-MM-dd}");
                if (r.Weight.HasValue)   sb.AppendLine($"  - Can nang: {r.Weight} kg");
                if (r.Height.HasValue)   sb.AppendLine($"  - Chieu cao: {r.Height} cm");
                if (r.Temperature.HasValue) sb.AppendLine($"  - Nhiet do: {r.Temperature} °C");
                if (r.HeartRate.HasValue) sb.AppendLine($"  - Nhip tim: {r.HeartRate} bpm");
                if (!string.IsNullOrWhiteSpace(r.Diagnosis))   sb.AppendLine($"  - Chan doan: {r.Diagnosis}");
                if (!string.IsNullOrWhiteSpace(r.Treatment))   sb.AppendLine($"  - Dieu tri: {r.Treatment}");
                if (!string.IsNullOrWhiteSpace(r.Notes))       sb.AppendLine($"  - Ghi chu: {r.Notes}");
            }
        }

        sb.AppendLine();
        sb.AppendLine($"## Yeu cau phan tich: {request.AnalysisType}");
        if (!string.IsNullOrWhiteSpace(request.AdditionalContext))
            sb.AppendLine($"## Mo ta them tu chu nuoi: {request.AdditionalContext}");

        sb.AppendLine();
        sb.AppendLine("## Cach tra loi");
        sb.AppendLine("Tra loi dung cau truc sau, bang tieng Viet, ro rang va de doc:");
        sb.AppendLine("1. **Tong quan suc khoe** - Tom tat tinh trang hien tai.");
        sb.AppendLine("2. **Diem dang chu y** - Liet ke cac dau hieu quan trong.");
        sb.AppendLine("3. **Khuyen nghi** - Dua ra huong xu ly cu the, de ap dung.");
        sb.AppendLine("4. **Rui ro can theo doi** - Nhac cac nguy co can canh giac.");
        sb.AppendLine("5. **Do tin cay** - Ghi theo dung mau: `Do tin cay: XX%`.");
        sb.AppendLine();
        sb.AppendLine("Luon nhac rang day chi la phan tich tham khao va khong thay the chan doan cua bac si thu y.");

        return sb.ToString();
    }

    private async Task<(string Text, int TokensUsed, string UsedModel)> CallGeminiAsync(string prompt)
    {
        var body = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.4,
                maxOutputTokens = 2048
            }
        };

        var modelsToTry = new List<string> { _model };
        modelsToTry.AddRange(FallbackModels.Where(model => !string.Equals(model, _model, StringComparison.OrdinalIgnoreCase)));

        HttpResponseMessage? lastResponse = null;

        foreach (var model in modelsToTry)
        {
            var url = string.Format(GeminiBaseUrl, model, _apiKey);

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                lastResponse = response;
                continue;
            }

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            var text = json
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? string.Empty;

            int tokensUsed = 0;
            if (json.TryGetProperty("usageMetadata", out var usage) &&
                usage.TryGetProperty("totalTokenCount", out var tokenEl))
            {
                tokensUsed = tokenEl.GetInt32();
            }

            return (text, tokensUsed, model);
        }

        if (lastResponse != null)
        {
            lastResponse.EnsureSuccessStatusCode();
        }

        throw new InvalidOperationException("No Gemini model could be used for analysis.");
    }

    private static string? ExtractSection(string text, params string[] sectionNames)
    {
        var idx = -1;
        var marker = string.Empty;

        foreach (var sectionName in sectionNames)
        {
            var candidate = $"**{sectionName}**";
            idx = text.IndexOf(candidate, StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                marker = candidate;
                break;
            }
        }

        if (idx < 0) return null;

        var start = idx + marker.Length;
        var nextSection = text.IndexOf("**", start, StringComparison.Ordinal);
        var end = nextSection > start ? nextSection : text.Length;

        return text[start..end].Trim();
    }

    private static decimal? ExtractConfidenceScore(string text)
    {
        var markers = new[] { "Do tin cay:", "Độ tin cậy:", "Confidence:" };

        foreach (var marker in markers)
        {
            var idx = text.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
            {
                continue;
            }

            var snippet = text[(idx + marker.Length)..].TrimStart();
            var digits = new string(snippet.TakeWhile(c => char.IsDigit(c) || c == '.').ToArray());
            if (decimal.TryParse(digits, out var value))
            {
                return Math.Clamp(value, 0, 100);
            }
        }

        return null;
    }

    private static string CalculateAge(DateTime dob)
    {
        var today = DateTime.UtcNow;
        var years = today.Year - dob.Year;
        var months = today.Month - dob.Month;
        if (months < 0) { years--; months += 12; }
        if (years > 0) return $"{years} tuoi";
        return $"{months} thang";
    }

    private static AIHealthAnalysisResponseDto MapToResponseDto(AIHealthAnalysis a, string petName) => new()
    {
        Id = a.Id,
        PetId = a.PetId,
        PetName = petName,
        AnalysisType = a.AnalysisType,
        AIResponse = a.AIResponse,
        Recommendations = a.Recommendations,
        ConfidenceScore = a.ConfidenceScore,
        AIModel = a.AIModel ?? string.Empty,
        IsReviewed = a.IsReviewed,
        ReviewNotes = a.ReviewNotes,
        CreatedAt = a.CreatedAt
    };

    private static string? GetFirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }

    private async Task TrySendAIAnalysisEmailAsync(Pet pet, AIHealthAnalysisResponseDto dto)
    {
        try
        {
            var owner = await _unitOfWork.Repository<User>().GetByIdAsync(pet.UserId);
            if (owner == null || string.IsNullOrWhiteSpace(owner.Email))
            {
                return;
            }

            var summary = dto.Recommendations;
            if (string.IsNullOrWhiteSpace(summary))
            {
                summary = dto.AIResponse;
            }

            if (summary.Length > 1200)
            {
                summary = summary[..1200] + "...";
            }

            await _emailService.SendAIAnalysisSummaryAsync(
                owner.Email,
                string.IsNullOrWhiteSpace(owner.FullName) ? "PetCare User" : owner.FullName,
                dto.PetName,
                dto.AnalysisType,
                summary,
                dto.ConfidenceScore,
                dto.CreatedAt);
        }
        catch
        {
            // Best effort only: AI analysis should not fail because of email issues.
        }
    }
}
