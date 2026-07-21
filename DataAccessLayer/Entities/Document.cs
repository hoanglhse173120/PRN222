using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities;

public partial class Document
{
    /// <summary> Khóa chính (Primary Key) của tài liệu </summary>
    public int DocumentId { get; set; }

    /// <summary> Khóa ngoại trỏ đến Môn học (Subject) </summary>
    [Required]
    public int SubjectId { get; set; }

    /// <summary> Tên File hiển thị gốc </summary>
    [Required]
    [StringLength(255)]
    public string FileName { get; set; } = null!;

    /// <summary> Định dạng (loại) File (VD: PDF, DOCX, TXT) </summary>
    [StringLength(50)]
    public string? FileType { get; set; }

    /// <summary> Đường dẫn tương đối/tuyệt đối để truy xuất File từ đĩa cứng Server </summary>
    [StringLength(1000)]
    public string? FilePath { get; set; }

    /// <summary> Dung lượng kích thước File lưu dưới dạng KB </summary>
    public long? FileSizeKb { get; set; }

    /// <summary> Cờ đánh dấu: True là đã xong quá trình cắt Chunk và gắn Vector Indexing </summary>
    public bool? IsIndexed { get; set; }

    /// <summary> Dấu thời gian hệ thống lưu lại lúc File upload hoàn tất </summary>
    public DateTime? UploadedAt { get; set; }

    public string? UploadedByUserId { get; set; }

    public virtual Microsoft.AspNetCore.Identity.IdentityUser? UploadedByUser { get; set; }

    /// <summary> Navigation Property: Danh sách tất cả các Chunk con phụ thuộc tài liệu này </summary>
    public virtual ICollection<DocumentChunk> DocumentChunks { get; set; } = new List<DocumentChunk>();

    public virtual Subject Subject { get; set; } = null!;
}
