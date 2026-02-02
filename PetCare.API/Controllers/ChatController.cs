namespace PetCare.API.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCare.Application.DTOs.Chat;
using PetCare.Application.Services.Interfaces;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpPost("sessions")]
    public async Task<IActionResult> CreateSession([FromBody] CreateChatSessionDto dto)
    {
        var result = await _chatService.CreateSessionAsync(dto);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }
        return Ok(result.Data);
    }

    [HttpGet("sessions/{sessionId}")]
    public async Task<IActionResult> GetSessionById(Guid sessionId)
    {
        var result = await _chatService.GetSessionByIdAsync(sessionId);
        if (!result.Success)
        {
            return NotFound(new { message = result.Message });
        }
        return Ok(result.Data);
    }

    [HttpGet("sessions/user/my-sessions")]
    [Authorize]
    public async Task<IActionResult> GetMySessions([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid user token" });
        }

        var result = await _chatService.GetUserSessionsAsync(userId, pageNumber, pageSize);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }
        return Ok(result.Data);
    }

    [HttpGet("sessions/active")]
    [Authorize(Roles = "Admin,Manager,Staff")]
    public async Task<IActionResult> GetActiveSessions([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _chatService.GetActiveSessionsAsync(pageNumber, pageSize);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }
        return Ok(result.Data);
    }

    [HttpPost("messages")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
    {
        var result = await _chatService.SendMessageAsync(dto);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }
        return Ok(result.Data);
    }

    [HttpGet("sessions/{sessionId}/messages")]
    public async Task<IActionResult> GetSessionMessages(Guid sessionId)
    {
        var result = await _chatService.GetSessionMessagesAsync(sessionId);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }
        return Ok(result.Data);
    }

    [HttpPut("sessions/{sessionId}/end")]
    public async Task<IActionResult> EndSession(Guid sessionId)
    {
        var result = await _chatService.EndSessionAsync(sessionId);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }
        return Ok(new { message = "Đã kết thúc phiên chat", success = true });
    }

    [HttpPost("bot/response")]
    public async Task<IActionResult> GetBotResponse([FromBody] BotMessageRequest request)
    {
        var result = await _chatService.GetBotResponseAsync(request.Message, request.SessionId);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }
        return Ok(result.Data);
    }
}

public class BotMessageRequest
{
    public string Message { get; set; } = string.Empty;
    public Guid SessionId { get; set; }
}
