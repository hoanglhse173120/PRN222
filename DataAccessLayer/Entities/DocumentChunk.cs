using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities;

public class DocumentChunk
{
    [Key]
    public int ChunkID { get; set; }
    public int DocumentID { get; set; }
    public string ChunkContent { get; set; } = null!;
    public int? ChunkIndex { get; set; }
    public int? PageNumber { get; set; }
    public string? Embedding { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public Document Document { get; set; } = null!;
    public ICollection<MessageSource> MessageSources { get; set; } = new List<MessageSource>();
}
