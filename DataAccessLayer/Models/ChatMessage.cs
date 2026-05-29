using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Models;

public class ChatMessage
{
    [Key]
    public int MessageID { get; set; }
    public int SessionID { get; set; }
    public string? Role { get; set; }   // 'user' | 'assistant'
    public string MessageText { get; set; } = null!;
    public DateTime Timestamp { get; set; } = DateTime.Now;

    // Navigation
    public ChatSession ChatSession { get; set; } = null!;
    public ICollection<MessageSource> MessageSources { get; set; } = new List<MessageSource>();
}
