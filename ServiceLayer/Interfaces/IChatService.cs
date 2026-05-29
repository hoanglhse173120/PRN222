using ServiceLayer.DTOs;

namespace ServiceLayer.Interfaces;

public interface IChatService
{
    Task<IEnumerable<ChatSessionDto>> GetAllSessionsAsync();
    Task<ChatSessionDto> CreateSessionAsync(string? sessionName = null);
    Task<ChatSessionDto?> GetSessionWithMessagesAsync(int sessionId);
    Task<ChatMessageDto> AddMessageAsync(int sessionId, string role, string messageText);
    Task DeleteSessionAsync(int sessionId);
}
