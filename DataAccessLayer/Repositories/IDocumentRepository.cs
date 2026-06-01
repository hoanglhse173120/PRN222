using DataAccessLayer.Entities;

namespace DataAccessLayer.Repositories;

public interface IDocumentRepository : IRepository<Document>
{
    Task<IEnumerable<Document>> GetBySubjectAsync(int subjectId);
    Task<IEnumerable<Document>> GetIndexedDocumentsAsync();
    Task<Document?> GetByIdWithChunksAsync(int documentId);
}
