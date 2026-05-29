using DataAccessLayer.Context;
using DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories;

public class DocumentRepository : Repository<Document>, IDocumentRepository
{
    public DocumentRepository(ChatbotDbContext context) : base(context) { }

    public async Task<IEnumerable<Document>> GetBySubjectAsync(int subjectId)
        => await _context.Documents
            .Where(d => d.SubjectID == subjectId)
            .Include(d => d.Subject)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();

    public async Task<IEnumerable<Document>> GetIndexedDocumentsAsync()
        => await _context.Documents
            .Where(d => d.IsIndexed)
            .Include(d => d.Subject)
            .ToListAsync();
}
