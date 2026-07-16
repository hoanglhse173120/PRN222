using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities;

public class ChunkingConfig
{
    [Key]
    public int Id { get; set; } // Luôn là 1
    public string Strategy { get; set; } = "Words"; // "Words", "Paragraphs", "Characters"
    public int MaxSize { get; set; } = 500;
    public int Overlap { get; set; } = 50;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
