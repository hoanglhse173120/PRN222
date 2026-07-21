using System;
using System.Collections.Generic;

namespace DataAccessLayer.Entities;

public partial class DocumentChunk
{
    /// <summary> Mã định danh duy nhất của Chunk </summary>
    public int ChunkId { get; set; }

    /// <summary> ID tài liệu cha liên kết </summary>
    public int DocumentId { get; set; }

    /// <summary> Nội dung gốc của Chunk để cung cấp ngữ cảnh cho AI </summary>
    public string ChunkContent { get; set; } = null!;

    /// <summary> Số thứ tự hiển thị của Chunk từ trên xuống dưới trong tài liệu </summary>
    public int? ChunkIndex { get; set; }

    /// <summary> Đánh dấu vị trí trang gốc để hiển thị trích dẫn (Citations) </summary>
    public int? PageNumber { get; set; }

    /// <summary> Ngày giờ hoàn thiện trích xuất Chunk </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary> Chuỗi sao lưu dạng Matrix Vector đặc trưng (Embedding Data) do AI sinh ra </summary>
    public string? Embedding { get; set; }

    public virtual Document Document { get; set; } = null!;

    /// <summary> Bảng mapping các tin nhắn mà con Bot đã dùng đoạn Chunk này làm tài liệu tham khảo </summary>
    public virtual ICollection<MessageSource> MessageSources { get; set; } = new List<MessageSource>();
}
