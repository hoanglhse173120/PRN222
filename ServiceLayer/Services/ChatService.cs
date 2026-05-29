using DataAccessLayer.Entities;
using DataAccessLayer.Repositories;
using ServiceLayer.Interfaces;
using ServiceLayer.DTOs;
using System.Linq;

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

    public async Task<IEnumerable<ChatSessionDto>> GetAllSessionsAsync()
    {
        var sessions = await _sessionRepo.GetAllOrderedAsync();
        return sessions.Select(MapSessionToDto);
    }

    public async Task<ChatSessionDto> CreateSessionAsync(string? sessionName = null)
    {
        var session = new ChatSession
        {
            SessionName = sessionName ?? "Phiên trò chuyện mới",
            CreatedAt = DateTime.Now
        };
        await _sessionRepo.AddAsync(session);
        await _sessionRepo.SaveChangesAsync();
        return MapSessionToDto(session);
    }

    public async Task<ChatSessionDto?> GetSessionWithMessagesAsync(int sessionId)
    {
        var session = await _sessionRepo.GetWithMessagesAsync(sessionId);
        if (session == null) return null;
        return MapSessionToDto(session);
    }

    public async Task<ChatMessageDto> AddMessageAsync(int sessionId, string role, string messageText)
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
        return MapMessageToDto(message);
    }

    private ChatSessionDto MapSessionToDto(ChatSession session)
    {
        return new ChatSessionDto
        {
            SessionID = session.SessionID,
            SessionName = session.SessionName,
            CreatedAt = session.CreatedAt,
            ChatMessages = session.ChatMessages?.Select(MapMessageToDto).ToList() ?? new List<ChatMessageDto>()
        };
    }

    private ChatMessageDto MapMessageToDto(ChatMessage message)
    {
        return new ChatMessageDto
        {
            MessageID = message.MessageID,
            SessionID = message.SessionID,
            Role = message.Role,
            MessageText = message.MessageText,
            Timestamp = message.Timestamp
        };
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
