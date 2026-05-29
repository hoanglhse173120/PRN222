using DataAccessLayer.Context;
using DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories;

public class ChatSessionRepository : Repository<ChatSession>, IChatSessionRepository
{
    public ChatSessionRepository(ChatbotDbContext context) : base(context) { }

    public async Task<ChatSession?> GetWithMessagesAsync(int sessionId)
        => await _context.ChatSessions
            .Include(s => s.ChatMessages.OrderBy(m => m.Timestamp))
            .FirstOrDefaultAsync(s => s.SessionID == sessionId);

    public async Task<IEnumerable<ChatSession>> GetAllOrderedAsync()
        => await _context.ChatSessions
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
}
