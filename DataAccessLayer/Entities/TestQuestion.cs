using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities;

public class TestQuestion
{
    [Key]
    public int QuestionID { get; set; }
    public int? SubjectID { get; set; }
    public string QuestionText { get; set; } = null!;
    public string GroundTruth { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public Subject? Subject { get; set; }
    public ICollection<BenchmarkResult> BenchmarkResults { get; set; } = new List<BenchmarkResult>();
}
