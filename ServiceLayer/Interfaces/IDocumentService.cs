using ServiceLayer.DTOs;

namespace ServiceLayer.Interfaces;

public interface IDocumentService
{
    /// <summary> Lấy toàn bộ danh sách tài liệu hiện có trong hệ thống </summary>
    Task<IEnumerable<DocumentDto>> GetAllAsync();
    
    /// <summary> Lấy danh sách tài liệu thuộc về một môn học cụ thể </summary>
    Task<IEnumerable<DocumentDto>> GetBySubjectAsync(int subjectId);
    
    /// <summary> Truy xuất thông tin cơ bản của một tài liệu theo ID </summary>
    Task<DocumentDto?> GetByIdAsync(int id);
    
    /// <summary> Truy xuất chi tiết một tài liệu kèm theo toàn bộ danh sách các chunks của nó </summary>
    Task<DocumentDetailsDto?> GetDetailsWithChunksAsync(int id);
    
    /// <summary> Tạo mới file tài liệu vào hệ thống (lưu siêu dữ liệu, chưa thực hiện Chunk/Index) </summary>
    Task<DocumentDto> UploadAsync(int subjectId, string fileName, string fileType, string filePath, long? fileSizeKB, string userId);
    
    /// <summary> Đánh dấu một tài liệu là đã hoàn tất quá trình Index (thêm vào Vector DB thành công) </summary>
    Task MarkAsIndexedAsync(int documentId);
    
    /// <summary> Xóa tài liệu khỏi CSDL đồng thời xóa file thực tế lưu trong hệ thống máy chủ </summary>
    Task DeleteAsync(int id, string webRootPath);

    /// <summary>
    /// Extract text từ file → chunk → lưu DocumentChunks → đánh dấu IsIndexed = true
    /// </summary>
    Task<int> IndexDocumentAsync(int documentId, string webRootPath);
}
