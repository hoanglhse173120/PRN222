namespace ServiceLayer.DTOs;

public class DocumentChunkDto
{
    public int ChunkID { get; set; }
    public int DocumentID { get; set; }
    public int? ChunkIndex { get; set; }
    public string ChunkContent { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public bool HasEmbedding => !string.IsNullOrEmpty(EmbeddingJson);
    public string? EmbeddingJson { get; set; }
}
