using ServiceLayer.DTOs;

namespace PresentationLayer.ViewModels;

public class ChatSessionViewModel
{
    public ChatSessionDto Session { get; set; } = null!;
    public string NewMessage { get; set; } = string.Empty;
    public IEnumerable<ChatSessionDto> AllSessions { get; set; } = [];
}
