namespace ServiceLayer.DTOs;

public class DocumentDto
{
    public int DocumentID { get; set; }
    public int SubjectID { get; set; }
    public string FileName { get; set; } = null!;
    public string? FileType { get; set; }
    public string? FilePath { get; set; }
    public long? FileSizeKB { get; set; }
    public bool IsIndexed { get; set; }
    public DateTime UploadedAt { get; set; }
    
    // Optional reference if needed
    public SubjectDto? Subject { get; set; }
}
