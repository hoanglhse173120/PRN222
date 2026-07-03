using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities;

public partial class ChatSession
{
    public int SessionId { get; set; }

    [StringLength(255)]
    public string? SessionName { get; set; }

    public DateTime? CreatedAt { get; set; }

    [StringLength(450)]
    public string? UserId { get; set; }

    public virtual Microsoft.AspNetCore.Identity.IdentityUser? User { get; set; }

    public int? SubjectId { get; set; }
    public virtual Subject? Subject { get; set; }

    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
}
