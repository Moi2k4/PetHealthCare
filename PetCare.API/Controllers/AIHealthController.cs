using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCare.Application.Services.Interfaces;

namespace PetCare.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    /// <summary>
    /// Analyze pet health from image
    /// </summary>
    [HttpPost("analyze")]
    public async Task<IActionResult> AnalyzeHealth([FromForm] IFormFile imageFile, [FromForm] Guid petId)
    {
        if (imageFile == null || imageFile.Length == 0)
            return BadRequest("Image file is required");

        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        using var memoryStream = new MemoryStream();
        await imageFile.CopyToAsync(memoryStream);
        var imageBytes = memoryStream.ToArray();

        var result = await _aiHealthService.AnalyzePetHealthAsync(petId, userId, imageBytes);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get AI health analysis history for a pet
    /// </summary>
    [HttpGet("pet/{petId}/history")]
    public async Task<IActionResult> GetPetAnalysisHistory(Guid petId)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var result = await _aiHealthService.GetPetAnalysisHistoryAsync(petId, userId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get AI health analysis by ID
    /// </summary>
    [HttpGet("{analysisId}")]
    public async Task<IActionResult> GetAnalysisById(Guid analysisId)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var result = await _aiHealthService.GetAnalysisByIdAsync(analysisId, userId);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Delete AI health analysis
    /// </summary>
    [HttpDelete("{analysisId}")]
    public async Task<IActionResult> DeleteAnalysis(Guid analysisId)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var result = await _aiHealthService.DeleteAnalysisAsync(analysisId, userId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
