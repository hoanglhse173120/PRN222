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
    private readonly IRepository<MessageSource> _messageSourceRepo;

    public ChatService(
        IChatSessionRepository sessionRepo,
        IRepository<ChatMessage> messageRepo,
        IRepository<MessageSource> messageSourceRepo)
    {
        _sessionRepo = sessionRepo;
        _messageRepo = messageRepo;
        _messageSourceRepo = messageSourceRepo;
    }

    public async Task<IEnumerable<ChatSessionDto>> GetAllSessionsByUserAsync(string userId)
    {
        var sessions = await _sessionRepo.GetAllOrderedByUserAsync(userId);
        return sessions.Select(MapSessionToDto);
    }

    public async Task<ChatSessionDto> CreateSessionAsync(string userId, string? sessionName = null)
    {
        var session = new ChatSession
        {
            UserId = userId,
            SessionName = sessionName ?? "Phiên trò chuyện mới",
            CreatedAt = DateTime.Now
        };
        await _sessionRepo.AddAsync(session);
        await _sessionRepo.SaveChangesAsync();
        return MapSessionToDto(session);
    }

    public async Task<ChatSessionDto?> GetSessionWithMessagesAsync(int sessionId, string userId)
    {
        var session = await _sessionRepo.GetWithMessagesAsync(sessionId, userId);
        if (session == null) return null;
        return MapSessionToDto(session);
    }

    public async Task<ChatMessageDto> AddMessageAsync(int sessionId, string role, string messageText)
    {
        var message = new ChatMessage
        {
            SessionId = sessionId,
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
            SessionID = session.SessionId,
            SessionName = session.SessionName,
            CreatedAt = session.CreatedAt ?? DateTime.MinValue,
            ChatMessages = session.ChatMessages?.Select(MapMessageToDto).ToList() ?? new List<ChatMessageDto>()
        };
    }

    private ChatMessageDto MapMessageToDto(ChatMessage message)
    {
        return new ChatMessageDto
        {
            MessageID = message.MessageId,
            SessionID = message.SessionId,
            Role = message.Role,
            MessageText = message.MessageText,
            Timestamp = message.Timestamp ?? DateTime.MinValue
        };
    }

    public async Task DeleteSessionAsync(int sessionId, string userId)
    {
        var session = await _sessionRepo.GetByIdAsync(sessionId);
        if (session != null && session.UserId == userId)
        {
            _sessionRepo.Delete(session);
            await _sessionRepo.SaveChangesAsync();
        }
    }

    public async Task RenameSessionAsync(int sessionId, string userId, string newName)
    {
        var session = await _sessionRepo.GetByIdAsync(sessionId);
        if (session != null && session.UserId == userId)
        {
            session.SessionName = newName;
            await _sessionRepo.SaveChangesAsync();
        }
    }

    public async Task<ChatMessageDto> AddMessageWithSourcesAsync(
        int sessionId, string role, string messageText,
        List<RagChunkResultDto> sources)
    {
        var message = new ChatMessage
        {
            SessionId = sessionId,
            Role = role,
            MessageText = messageText,
            Timestamp = DateTime.Now
        };
        await _messageRepo.AddAsync(message);
        await _messageRepo.SaveChangesAsync();

        // Lưu sources (MessageSource) để trích dẫn nguồn
        foreach (var src in sources)
        {
            var ms = new MessageSource
            {
                MessageId = message.MessageId,
                ChunkId = src.ChunkID,
                RelevanceScore = src.Score
            };
            await _messageSourceRepo.AddAsync(ms);
        }
        if (sources.Any())
            await _messageSourceRepo.SaveChangesAsync();

        return MapMessageToDto(message);
    }
}
