using ServiceLayer.DTOs;

namespace ServiceLayer.Interfaces;

public interface IDocumentService
{
    Task<IEnumerable<DocumentDto>> GetAllAsync();
    Task<IEnumerable<DocumentDto>> GetBySubjectAsync(int subjectId);
    Task<DocumentDto?> GetByIdAsync(int id);
    Task<DocumentDetailsDto?> GetDetailsWithChunksAsync(int id);
    Task<DocumentDto> UploadAsync(int subjectId, string fileName, string fileType, string filePath, long? fileSizeKB, string userId);
    Task MarkAsIndexedAsync(int documentId);
    Task DeleteAsync(int id, string webRootPath);

    /// <summary>
    /// Extract text từ file → chunk → lưu DocumentChunks → đánh dấu IsIndexed = true
    /// </summary>
    Task<int> IndexDocumentAsync(int documentId, string webRootPath);
}
