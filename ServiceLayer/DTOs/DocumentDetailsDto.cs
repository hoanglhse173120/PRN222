namespace ServiceLayer.DTOs;

public class DocumentDetailsDto
{
    public int DocumentID { get; set; }
    public string FileName { get; set; } = null!;
    public string? FileType { get; set; }
    public string? FilePath { get; set; }
    public long? FileSizeKB { get; set; }
    public bool IsIndexed { get; set; }
    public DateTime UploadedAt { get; set; }

    public SubjectDto? Subject { get; set; }
    public List<DocumentChunkDto> Chunks { get; set; } = new();

    public int TotalChunks => Chunks.Count;
    public int ChunksWithEmbedding => Chunks.Count(c => c.HasEmbedding);
}
