namespace ServiceLayer.DTOs;

public class RagChunkResultDto
{
    public int ChunkID { get; set; }
    public int DocumentID { get; set; }
    public string FileName { get; set; } = null!;
    public string? SubjectName { get; set; }
    public string ChunkContent { get; set; } = null!;
    public int? ChunkIndex { get; set; }
    public double Score { get; set; }
}

public class RagResponseDto
{
    public string Answer { get; set; } = null!;
    public List<RagChunkResultDto> Sources { get; set; } = new();
    public bool IsFromDocuments { get; set; } = true;
}
