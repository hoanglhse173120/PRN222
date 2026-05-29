using DataAccessLayer.Models;

namespace PresentationLayer.ViewModels;

public class ChatSessionViewModel
{
    public ChatSession Session { get; set; } = null!;
    public string NewMessage { get; set; } = string.Empty;
}
