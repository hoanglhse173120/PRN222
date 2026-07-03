using DataAccessLayer.Context;
using DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories;

public class ChatSessionRepository : Repository<ChatSession>, IChatSessionRepository
{
    public ChatSessionRepository(ChatbotDbContext context) : base(context) { }

    public async Task<ChatSession?> GetWithMessagesAsync(int sessionId, string userId)
        => await _context.ChatSessions
            .Include(s => s.Subject)
            .Include(s => s.ChatMessages.OrderBy(m => m.Timestamp))
                .ThenInclude(m => m.MessageSources)
                    .ThenInclude(ms => ms.Chunk)
                        .ThenInclude(c => c.Document)
                            .ThenInclude(d => d.Subject)
            .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.UserId == userId);

    public async Task<IEnumerable<ChatSession>> GetAllOrderedByUserAsync(string userId)
        => await _context.ChatSessions
            .Include(s => s.Subject)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
}
