using System;
using System.Collections.Generic;

namespace DataAccessLayer.Entities;

public partial class DocumentChunk
{
    public int ChunkId { get; set; }

    public int DocumentId { get; set; }

    public string ChunkContent { get; set; } = null!;

    public int? ChunkIndex { get; set; }

    public int? PageNumber { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Embedding { get; set; }

    public virtual Document Document { get; set; } = null!;

    public virtual ICollection<MessageSource> MessageSources { get; set; } = new List<MessageSource>();
}
