using System;
using System.Collections.Generic;

namespace DataAccessLayer.Entities;

public partial class ChatSession
{
    public int SessionId { get; set; }

    public string? SessionName { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? UserId { get; set; }

    public virtual Microsoft.AspNetCore.Identity.IdentityUser? User { get; set; }

    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
}
