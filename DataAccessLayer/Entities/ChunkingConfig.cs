using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities;

public class ChunkingConfig
{
    /// <summary> Là Primary Key mang tính chất hình thức, thông thường luôn set là 1 (Singleton Config) </summary>
    [Key]
    public int Id { get; set; }
    
    /// <summary> Chọn chiến lược phân tích gồm: "Words", "Paragraphs", "Characters" </summary>
    public string Strategy { get; set; } = "Words";
    
    /// <summary> Giới hạn điểm cắt tối đa cho một phân mảnh </summary>
    public int MaxSize { get; set; } = 500;
    
    /// <summary> Số lượng đệm giao nhau (ngữ cảnh trùng lặp) giữa Chunk cũ và Chunk mới tiến tới </summary>
    public int Overlap { get; set; } = 50;
    
    /// <summary> Thời gian User thực hiện ghi đè cấu hình mới nhất </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
