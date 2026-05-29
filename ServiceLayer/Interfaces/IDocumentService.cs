using DataAccessLayer.Models;

namespace ServiceLayer.Interfaces;

public interface IDocumentService
{
    Task<IEnumerable<Document>> GetAllAsync();
    Task<IEnumerable<Document>> GetBySubjectAsync(int subjectId);
    Task<Document?> GetByIdAsync(int id);
    Task<Document> UploadAsync(int subjectId, string fileName, string fileType, string filePath, long fileSizeKB);
    Task MarkAsIndexedAsync(int documentId);
    Task DeleteAsync(int id);

    /// <summary>
    /// Extract text từ file → chunk → lưu DocumentChunks → đánh dấu IsIndexed = true
    /// </summary>
    Task<int> IndexDocumentAsync(int documentId, string webRootPath, int chunkSize = 500, int overlap = 50);
}
