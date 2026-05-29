using DataAccessLayer.Models;

namespace ServiceLayer.Interfaces;

public interface IChatService
{
    Task<IEnumerable<ChatSession>> GetAllSessionsAsync();
    Task<ChatSession> CreateSessionAsync(string? sessionName = null);
    Task<ChatSession?> GetSessionWithMessagesAsync(int sessionId);
    Task<ChatMessage> AddMessageAsync(int sessionId, string role, string messageText);
    Task DeleteSessionAsync(int sessionId);
}
