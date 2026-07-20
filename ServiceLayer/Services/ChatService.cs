using DataAccessLayer.Entities;
using DataAccessLayer.Repositories;
using ServiceLayer.Interfaces;
using ServiceLayer.DTOs;
using System.Linq;

namespace ServiceLayer.Services;

/// <summary>
/// Service quản lý nghiệp vụ Hội thoại (Session) và Tin nhắn (Message) cho Chatbot.
/// Hỗ trợ luồng dữ liệu quản lý phiên trò chuyện, lưu thông tin trích dẫn nguồn khi chat.
/// </summary>
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

    /// <summary>
    /// Lấy toàn bộ danh sách phiên trò chuyện của một tải khoản cụ thể. 
    /// Dùng cho thanh Menu Sidebar bên trái.
    /// </summary>
    public async Task<IEnumerable<ChatSessionDto>> GetAllSessionsByUserAsync(string userId)
    {
        var sessions = await _sessionRepo.GetAllOrderedByUserAsync(userId);
        return sessions.Select(MapSessionToDto);
    }

    /// <summary>
    /// Tạo nhanh một phiên chat mới lưu cơ sở dữ liệu. 
    /// Phiên này có thể gắn liền với môn học để filter tri thức RAG hoặc không có môn học (tìm tự do).
    /// </summary>
    public async Task<ChatSessionDto> CreateSessionAsync(string userId, int? subjectId, string? sessionName = null)
    {
        var session = new ChatSession
        {
            UserId = userId,
            SubjectId = subjectId,
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
            Timestamp = DateTime.Now,
            TokenCount = (messageText?.Length ?? 0) / 4
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
            SessionName = session.SessionName ?? "Phiên chat mới",
            CreatedAt = session.CreatedAt ?? DateTime.MinValue,
            SubjectId = session.SubjectId,
            SubjectName = session.Subject?.SubjectName,
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
            Timestamp = message.Timestamp ?? DateTime.MinValue,
            Sources = message.MessageSources?.Select(ms => new RagChunkResultDto
            {
                ChunkID = ms.ChunkId,
                Score = ms.RelevanceScore ?? 0,
                FileName = ms.Chunk?.Document?.FileName ?? "Tài liệu không xác định",
                FilePath = ms.Chunk?.Document?.FilePath,
                SubjectName = ms.Chunk?.Document?.Subject?.SubjectName,
                ChunkContent = ms.Chunk?.ChunkContent ?? ""
            }).ToList()
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

    /// <summary>
    /// Lưu vào hệ thống một tin nhắn đến từ AI hoặc người dùng, 
    /// kèm thêm thông tin nguồn kham khảo liên kết (các đoạn chunk lấy từ tài liệu) 
    /// đem lại khả năng minh bạch (Citation) của kết quả trợ lý AI sinh ra.
    /// </summary>
    public async Task<ChatMessageDto> AddMessageWithSourcesAsync(
        int sessionId, string role, string messageText,
        List<RagChunkResultDto> sources)
    {
        var contextLength = sources?.Sum(s => s.ChunkContent?.Length ?? 0) ?? 0;
        var message = new ChatMessage
        {
            SessionId = sessionId,
            Role = role,
            MessageText = messageText,
            Timestamp = DateTime.Now,
            TokenCount = ((messageText?.Length ?? 0) + contextLength) / 4
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
