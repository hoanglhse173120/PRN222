using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities;

public class BenchmarkResult
{
    [Key]
    public int ResultID { get; set; }
    public int ConfigID { get; set; }
    public int QuestionID { get; set; }
    public string? ModelResponse { get; set; }
    public double? Faithfulness { get; set; }
    public double? AnswerRelevance { get; set; }
    public double? ContextPrecision { get; set; }
    public double? ContextRecall { get; set; }
    public DateTime EvaluatedAt { get; set; } = DateTime.Now;

    // Navigation
    public ExperimentConfig ExperimentConfig { get; set; } = null!;
    public TestQuestion TestQuestion { get; set; } = null!;
}
