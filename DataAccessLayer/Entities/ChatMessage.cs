using System;
using System.Collections.Generic;

namespace DataAccessLayer.Entities;

public partial class ChatMessage
{
    public int MessageId { get; set; }

    public int SessionId { get; set; }

    public string? Role { get; set; }

    public string MessageText { get; set; } = null!;

    public DateTime? Timestamp { get; set; }

    public virtual ICollection<MessageSource> MessageSources { get; set; } = new List<MessageSource>();

    public virtual ChatSession Session { get; set; } = null!;
}
