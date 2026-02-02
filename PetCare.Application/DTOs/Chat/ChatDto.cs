namespace PetCare.Application.DTOs.Chat;

public class ChatSessionDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime SessionStart { get; set; }
    public DateTime? SessionEnd { get; set; }
    public bool IsActive { get; set; }
    public int MessageCount { get; set; }
    public ChatMessageDto? LastMessage { get; set; }
}

public class ChatMessageDto
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public string SenderType { get; set; } = string.Empty;
    public string MessageText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateChatSessionDto
{
    public Guid? UserId { get; set; }
}

public class SendMessageDto
{
    public Guid SessionId { get; set; }
    public string MessageText { get; set; } = string.Empty;
    public string SenderType { get; set; } = "user"; // user or bot
}

public class ChatBotResponseDto
{
    public string Response { get; set; } = string.Empty;
    public string? Intent { get; set; }
    public List<string>? SuggestedActions { get; set; }
}
