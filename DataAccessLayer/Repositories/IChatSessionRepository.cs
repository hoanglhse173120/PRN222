using DataAccessLayer.Entities;

namespace DataAccessLayer.Repositories;

public interface IChatSessionRepository : IRepository<ChatSession>
{
    Task<ChatSession?> GetWithMessagesAsync(int sessionId);
    Task<IEnumerable<ChatSession>> GetAllOrderedAsync();
}
