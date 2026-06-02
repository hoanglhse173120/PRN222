using System;
using System.Collections.Generic;

namespace DataAccessLayer.Entities;

public partial class ExperimentConfig
{
    public int ConfigId { get; set; }

    public string ConfigName { get; set; } = null!;

    public string? ApproachType { get; set; }

    public string? EmbeddingModel { get; set; }

    public string? ChunkingStrategy { get; set; }

    public int? ChunkSize { get; set; }

    public int? ChunkOverlap { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<BenchmarkResult> BenchmarkResults { get; set; } = new List<BenchmarkResult>();
}
