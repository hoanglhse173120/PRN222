namespace DataAccessLayer.Entities;

public class Document
{
    public int DocumentID { get; set; }
    public int SubjectID { get; set; }
    public string FileName { get; set; } = null!;
    public string? FileType { get; set; }
    public string? FilePath { get; set; }
    public long? FileSizeKB { get; set; }
    public bool IsIndexed { get; set; } = false;
    public DateTime UploadedAt { get; set; } = DateTime.Now;

    // Navigation
    public Subject Subject { get; set; } = null!;
    public ICollection<DocumentChunk> DocumentChunks { get; set; } = new List<DocumentChunk>();
}
