namespace ServiceLayer.DTOs;

public class TestQuestionDto
{
    public int QuestionId { get; set; }
    public int? SubjectId { get; set; }
    public string QuestionText { get; set; } = null!;
    public string GroundTruth { get; set; } = null!;
    public DateTime? CreatedAt { get; set; }
}
