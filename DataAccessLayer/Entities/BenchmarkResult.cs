using System;
using System.Collections.Generic;

namespace DataAccessLayer.Entities;

public partial class BenchmarkResult
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

    public virtual ExperimentConfig Config { get; set; } = null!;

    public virtual TestQuestion Question { get; set; } = null!;
}
