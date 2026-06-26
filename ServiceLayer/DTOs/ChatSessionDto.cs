namespace ServiceLayer.DTOs;

public class ChatSessionDto
{
    public int SessionID { get; set; }
    public string SessionName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public int? SubjectId { get; set; }
    public string? SubjectName { get; set; }
    public ICollection<ChatMessageDto> ChatMessages { get; set; } = new List<ChatMessageDto>();
}
