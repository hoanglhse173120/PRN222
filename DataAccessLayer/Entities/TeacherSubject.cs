namespace DataAccessLayer.Entities;

public class TeacherSubject
{
    public int Id { get; set; }

    public string TeacherId { get; set; } = null!;

    public int SubjectId { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.Now;

    // Navigation
    public virtual Subject Subject { get; set; } = null!;
}
