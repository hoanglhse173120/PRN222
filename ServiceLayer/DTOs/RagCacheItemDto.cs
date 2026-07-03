namespace ServiceLayer.DTOs;

public class RagCacheItemDto
{
    public int ChunkId { get; set; }
    public int DocumentId { get; set; }
    public int? ChunkIndex { get; set; }
    public string ChunkContent { get; set; } = string.Empty;
    public string? Embedding { get; set; }
    public float[]? ParsedEmbedding { get; set; }
    
    // Subject Info
    public int? SubjectId { get; set; }
    public string? SubjectName { get; set; }
    
    // Document Info
    public string FileName { get; set; } = string.Empty;
}
