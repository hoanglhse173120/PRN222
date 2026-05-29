using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Models;

public class MessageSource
{
    [Key]
    public int SourceID { get; set; }
    public int MessageID { get; set; }
    public int ChunkID { get; set; }
    public double? RelevanceScore { get; set; }

    // Navigation
    public ChatMessage ChatMessage { get; set; } = null!;
    public DocumentChunk DocumentChunk { get; set; } = null!;
}
