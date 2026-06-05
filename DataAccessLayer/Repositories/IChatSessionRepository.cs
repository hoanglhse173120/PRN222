using DataAccessLayer.Entities;

namespace DataAccessLayer.Repositories;

public interface IChatSessionRepository : IRepository<ChatSession>
{
    Task<ChatSession?> GetWithMessagesAsync(int sessionId, string userId);
    Task<IEnumerable<ChatSession>> GetAllOrderedByUserAsync(string userId);
}
