using DataAccessLayer.Models;
using DataAccessLayer.Repositories;
using ServiceLayer.Interfaces;

namespace ServiceLayer.Services;

public class ChatService : IChatService
{
    private readonly IChatSessionRepository _sessionRepo;
    private readonly IRepository<ChatMessage> _messageRepo;

    public ChatService(IChatSessionRepository sessionRepo, IRepository<ChatMessage> messageRepo)
    {
        _sessionRepo = sessionRepo;
        _messageRepo = messageRepo;
    }

    public async Task<IEnumerable<ChatSession>> GetAllSessionsAsync()
        => await _sessionRepo.GetAllOrderedAsync();

    public async Task<ChatSession> CreateSessionAsync(string? sessionName = null)
    {
        var session = new ChatSession
        {
            SessionName = sessionName ?? "Phiên trò chuyện mới",
            CreatedAt = DateTime.Now
        };
        await _sessionRepo.AddAsync(session);
        await _sessionRepo.SaveChangesAsync();
        return session;
    }

    public async Task<ChatSession?> GetSessionWithMessagesAsync(int sessionId)
        => await _sessionRepo.GetWithMessagesAsync(sessionId);

    public async Task<ChatMessage> AddMessageAsync(int sessionId, string role, string messageText)
    {
        var message = new ChatMessage
        {
            SessionID = sessionId,
            Role = role,
            MessageText = messageText,
            Timestamp = DateTime.Now
        };
        await _messageRepo.AddAsync(message);
        await _messageRepo.SaveChangesAsync();
        return message;
    }

    public async Task DeleteSessionAsync(int sessionId)
    {
        var session = await _sessionRepo.GetByIdAsync(sessionId);
        if (session != null)
        {
            _sessionRepo.Delete(session);
            await _sessionRepo.SaveChangesAsync();
        }
    }
}
