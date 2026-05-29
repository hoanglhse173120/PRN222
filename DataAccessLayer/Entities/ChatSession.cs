using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities;

public class ChatSession
{
    [Key]
    public int SessionID { get; set; }
    public string SessionName { get; set; } = "Phiên trò chuyện mới";
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
}
