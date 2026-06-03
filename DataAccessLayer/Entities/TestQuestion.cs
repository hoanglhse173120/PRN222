using System;
using System.Collections.Generic;

namespace DataAccessLayer.Entities;

public partial class TestQuestion
{
    public int QuestionId { get; set; }

    public int? SubjectId { get; set; }

    public string QuestionText { get; set; } = null!;

    public string GroundTruth { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<BenchmarkResult> BenchmarkResults { get; set; } = new List<BenchmarkResult>();

    public virtual Subject? Subject { get; set; }
}
