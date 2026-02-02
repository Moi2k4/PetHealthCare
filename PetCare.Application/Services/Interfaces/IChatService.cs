namespace PetCare.Application.Services.Interfaces;

using PetCare.Application.Common;
using PetCare.Application.DTOs.Chat;

public interface IChatService
{
    Task<ServiceResult<ChatSessionDto>> CreateSessionAsync(CreateChatSessionDto dto);
    Task<ServiceResult<ChatSessionDto>> GetSessionByIdAsync(Guid sessionId);
    Task<ServiceResult<PagedResult<ChatSessionDto>>> GetUserSessionsAsync(Guid userId, int pageNumber = 1, int pageSize = 10);
    Task<ServiceResult<PagedResult<ChatSessionDto>>> GetActiveSessionsAsync(int pageNumber = 1, int pageSize = 10);
    Task<ServiceResult<ChatMessageDto>> SendMessageAsync(SendMessageDto dto);
    Task<ServiceResult<List<ChatMessageDto>>> GetSessionMessagesAsync(Guid sessionId);
    Task<ServiceResult<bool>> EndSessionAsync(Guid sessionId);
    Task<ServiceResult<ChatBotResponseDto>> GetBotResponseAsync(string message, Guid sessionId);
}
