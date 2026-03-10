using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCare.Application.DTOs.Health;
using PetCare.Application.Services.Interfaces;

namespace PetCare.API.Controllers;

[ApiController]
[Route("api/ai-health")]
[Authorize]
public class AIHealthController : ControllerBase
{
    private readonly IAIHealthService _aiHealthService;

    public AIHealthController(IAIHealthService aiHealthService)
    {
        _aiHealthService = aiHealthService;
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    /// <summary>
    /// Run a Google Gemini AI analysis on a pet's health records.
    /// AnalysisType options: HealthProfile | Recommendation | DiseaseRisk | Nutrition
    /// </summary>
    [HttpPost("analyse")]
    public async Task<IActionResult> Analyse([FromBody] AIHealthAnalysisRequestDto request)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _aiHealthService.AnalyseHealthAsync(request, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get the AI analysis history for a pet (most recent first).
    /// </summary>
    [HttpGet("history/{petId}")]
    public async Task<IActionResult> GetHistory(Guid petId)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _aiHealthService.GetAnalysisHistoryAsync(petId, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get a specific AI analysis record by its ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _aiHealthService.GetAnalysisByIdAsync(id, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
