using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Models;

public class ExperimentConfig
{
    [Key]
    public int ConfigID { get; set; }
    public string ConfigName { get; set; } = null!;
    public string? ApproachType { get; set; }       // 'RAG' | 'Fine-Tuned'
    public string? EmbeddingModel { get; set; }
    public string? ChunkingStrategy { get; set; }
    public int? ChunkSize { get; set; }
    public int? ChunkOverlap { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public ICollection<BenchmarkResult> BenchmarkResults { get; set; } = new List<BenchmarkResult>();
}
