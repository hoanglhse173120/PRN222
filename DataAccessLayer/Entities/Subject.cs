namespace DataAccessLayer.Entities;

public class Subject
{
    public int SubjectID { get; set; }
    public string SubjectName { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public ICollection<Document> Documents { get; set; } = new List<Document>();
    public ICollection<TestQuestion> TestQuestions { get; set; } = new List<TestQuestion>();
}
