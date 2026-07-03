using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities;

public partial class ChatMessage
{
    public int MessageId { get; set; }

    [Required]
    public int SessionId { get; set; }

    [StringLength(50)]
    public string? Role { get; set; }

    [Required]
    public string MessageText { get; set; } = null!;

    public DateTime? Timestamp { get; set; }

    public virtual ICollection<MessageSource> MessageSources { get; set; } = new List<MessageSource>();

    public virtual ChatSession Session { get; set; } = null!;
}
