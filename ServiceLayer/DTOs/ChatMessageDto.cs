namespace ServiceLayer.DTOs;

public class ChatMessageDto
{
    public int MessageID { get; set; }
    public int SessionID { get; set; }
    public string? Role { get; set; }
    public string MessageText { get; set; } = null!;
    public DateTime Timestamp { get; set; }
    public List<RagChunkResultDto>? Sources { get; set; }
}
