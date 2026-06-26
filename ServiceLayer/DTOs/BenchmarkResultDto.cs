namespace ServiceLayer.DTOs;

public class BenchmarkResultDto
{
    public int ResultId { get; set; }
    public int ConfigId { get; set; }
    public int QuestionId { get; set; }
    public string? ModelResponse { get; set; }
    public double? Faithfulness { get; set; }
    public double? AnswerRelevance { get; set; }
    public double? ContextPrecision { get; set; }
    public double? ContextRecall { get; set; }
    public DateTime? EvaluatedAt { get; set; }
}
