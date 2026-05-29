namespace ServiceLayer.DTOs;

public class SubjectDto
{
    public int SubjectID { get; set; }
    public string SubjectName { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
