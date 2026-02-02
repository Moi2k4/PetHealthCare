using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PetCare.Application.Common;
using PetCare.Application.DTOs.Chat;
using PetCare.Application.Services.Interfaces;
using PetCare.Domain.Entities;
using PetCare.Infrastructure.Repositories.Interfaces;

namespace PetCare.Application.Services.Implementations;

public class ChatService : IChatService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ChatService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ServiceResult<ChatSessionDto>> CreateSessionAsync(CreateChatSessionDto dto)
    {
        try
        {
            var session = new ChatSession
            {
                UserId = dto.UserId,
                SessionStart = DateTime.UtcNow,
                IsActive = true
            };

            await _unitOfWork.Repository<ChatSession>().AddAsync(session);
            await _unitOfWork.SaveChangesAsync();

            var sessionDto = _mapper.Map<ChatSessionDto>(session);
            return ServiceResult<ChatSessionDto>.SuccessResult(sessionDto, "Chat session created successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<ChatSessionDto>.FailureResult($"Error creating chat session: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ChatSessionDto>> GetSessionByIdAsync(Guid sessionId)
    {
        try
        {
            var sessionRepository = _unitOfWork.Repository<ChatSession>();
            var session = await sessionRepository.GetByIdAsync(sessionId);

            if (session == null)
            {
                return ServiceResult<ChatSessionDto>.FailureResult("Chat session not found");
            }

            var sessionDto = _mapper.Map<ChatSessionDto>(session);
            return ServiceResult<ChatSessionDto>.SuccessResult(sessionDto);
        }
        catch (Exception ex)
        {
            return ServiceResult<ChatSessionDto>.FailureResult($"Error retrieving chat session: {ex.Message}");
        }
    }

    public async Task<ServiceResult<PagedResult<ChatSessionDto>>> GetUserSessionsAsync(Guid userId, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var sessionRepository = _unitOfWork.Repository<ChatSession>();
            var (sessions, totalCount) = await sessionRepository.GetPagedAsync(
                pageNumber,
                pageSize,
                filter: s => s.UserId == userId,
                orderBy: q => q.OrderByDescending(s => s.SessionStart)
            );

            var sessionDtos = _mapper.Map<IEnumerable<ChatSessionDto>>(sessions);

            var pagedResult = new PagedResult<ChatSessionDto>
            {
                Items = sessionDtos,
                TotalCount = totalCount,
                Page = pageNumber,
                PageSize = pageSize
            };

            return ServiceResult<PagedResult<ChatSessionDto>>.SuccessResult(pagedResult);
        }
        catch (Exception ex)
        {
            return ServiceResult<PagedResult<ChatSessionDto>>.FailureResult($"Error retrieving user sessions: {ex.Message}");
        }
    }

    public async Task<ServiceResult<PagedResult<ChatSessionDto>>> GetActiveSessionsAsync(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var sessionRepository = _unitOfWork.Repository<ChatSession>();
            var (sessions, totalCount) = await sessionRepository.GetPagedAsync(
                pageNumber,
                pageSize,
                filter: s => s.IsActive,
                orderBy: q => q.OrderByDescending(s => s.SessionStart)
            );

            var sessionDtos = _mapper.Map<IEnumerable<ChatSessionDto>>(sessions);

            var pagedResult = new PagedResult<ChatSessionDto>
            {
                Items = sessionDtos,
                TotalCount = totalCount,
                Page = pageNumber,
                PageSize = pageSize
            };

            return ServiceResult<PagedResult<ChatSessionDto>>.SuccessResult(pagedResult);
        }
        catch (Exception ex)
        {
            return ServiceResult<PagedResult<ChatSessionDto>>.FailureResult($"Error retrieving active sessions: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ChatMessageDto>> SendMessageAsync(SendMessageDto dto)
    {
        try
        {
            var sessionRepository = _unitOfWork.Repository<ChatSession>();
            var session = await sessionRepository.GetByIdAsync(dto.SessionId);

            if (session == null)
            {
                return ServiceResult<ChatMessageDto>.FailureResult("Chat session not found");
            }

            if (!session.IsActive)
            {
                return ServiceResult<ChatMessageDto>.FailureResult("Chat session is closed");
            }

            var message = new ChatMessage
            {
                SessionId = dto.SessionId,
                SenderType = dto.SenderType,
                MessageText = dto.MessageText,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<ChatMessage>().AddAsync(message);
            await _unitOfWork.SaveChangesAsync();

            var messageDto = _mapper.Map<ChatMessageDto>(message);
            return ServiceResult<ChatMessageDto>.SuccessResult(messageDto, "Message sent successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<ChatMessageDto>.FailureResult($"Error sending message: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<ChatMessageDto>>> GetSessionMessagesAsync(Guid sessionId)
    {
        try
        {
            var messageRepository = _unitOfWork.Repository<ChatMessage>();
            var messages = await messageRepository.FindAsync(m => m.SessionId == sessionId);
            
            // Order by date
            var orderedMessages = messages.OrderBy(m => m.CreatedAt).ToList();

            var messageDtos = _mapper.Map<List<ChatMessageDto>>(orderedMessages);
            return ServiceResult<List<ChatMessageDto>>.SuccessResult(messageDtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<List<ChatMessageDto>>.FailureResult($"Error retrieving session messages: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> EndSessionAsync(Guid sessionId)
    {
        try
        {
            var sessionRepository = _unitOfWork.Repository<ChatSession>();
            var session = await sessionRepository.GetByIdAsync(sessionId);

            if (session == null)
            {
                return ServiceResult<bool>.FailureResult("Chat session not found");
            }

            session.IsActive = false;
            session.SessionEnd = DateTime.UtcNow;

            await sessionRepository.UpdateAsync(session);
            await _unitOfWork.SaveChangesAsync();

            return ServiceResult<bool>.SuccessResult(true, "Chat session ended successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult($"Error ending session: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ChatBotResponseDto>> GetBotResponseAsync(string message, Guid sessionId)
    {
        // Simple mock implementation of a bot response
        // In a real application, this would call an AI service or chatbot API
        await Task.Delay(500); // Simulate processing time

        var response = new ChatBotResponseDto
        {
            Response = $"Thank you for your message: \"{message}\". Our support team will be with you shortly.",
            Intent = "general_inquiry",
            SuggestedActions = new List<string> { "Book Appointment", "View Products", "Contact Support" }
        };

        // Automatically store the bot's response as a message
        var botMessage = new ChatMessage
        {
            SessionId = sessionId,
            SenderType = "bot",
            MessageText = response.Response,
            CreatedAt = DateTime.UtcNow
        };

        try 
        {
            await _unitOfWork.Repository<ChatMessage>().AddAsync(botMessage);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log error but verify we return the response
            // Log.Error($"Failed to save bot response: {ex.Message}");
        }

        return ServiceResult<ChatBotResponseDto>.SuccessResult(response);
    }
}
