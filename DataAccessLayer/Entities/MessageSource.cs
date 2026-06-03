using System;
using System.Collections.Generic;

namespace DataAccessLayer.Entities;

public partial class MessageSource
{
    public int SourceId { get; set; }

    public int MessageId { get; set; }

    public int ChunkId { get; set; }

    public double? RelevanceScore { get; set; }

    public virtual DocumentChunk Chunk { get; set; } = null!;

    public virtual ChatMessage Message { get; set; } = null!;
}
