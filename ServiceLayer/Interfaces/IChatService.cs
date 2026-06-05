using ServiceLayer.DTOs;

namespace ServiceLayer.Interfaces;

public interface IChatService
{
    Task<IEnumerable<ChatSessionDto>> GetAllSessionsByUserAsync(string userId);
    Task<ChatSessionDto> CreateSessionAsync(string userId, string? sessionName = null);
    Task<ChatSessionDto?> GetSessionWithMessagesAsync(int sessionId, string userId);
    Task<ChatMessageDto> AddMessageAsync(int sessionId, string role, string messageText);
    Task<ChatMessageDto> AddMessageWithSourcesAsync(int sessionId, string role, string messageText, List<ServiceLayer.DTOs.RagChunkResultDto> sources);
    Task RenameSessionAsync(int sessionId, string userId, string newName);
    Task DeleteSessionAsync(int sessionId, string userId);
}
