using DataAccessLayer.Context;
using DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories;

public class DocumentRepository : Repository<Document>, IDocumentRepository
{
    public DocumentRepository(ChatbotDbContext context) : base(context) { }

    // Override để luôn load Subject kèm theo
    public override async Task<IEnumerable<Document>> GetAllAsync()
        => await _context.Documents
            .Include(d => d.Subject)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();

    public async Task<IEnumerable<Document>> GetBySubjectAsync(int subjectId)
        => await _context.Documents
            .Where(d => d.SubjectId == subjectId)
            .Include(d => d.Subject)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();

    public async Task<Document?> GetByIdWithChunksAsync(int documentId)
        => await _context.Documents
            .Include(d => d.Subject)
            .Include(d => d.DocumentChunks.OrderBy(c => c.ChunkIndex))
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

    public async Task<IEnumerable<DocumentChunk>> GetAllIndexedChunksAsync()
        => await _context.DocumentChunks
            .Where(c => c.Embedding != null && c.Document.IsIndexed == true)
            .Include(c => c.Document)
                .ThenInclude(d => d.Subject)
            .ToListAsync();

    public async Task<IEnumerable<Document>> GetIndexedDocumentsAsync()
        => await _context.Documents
            .Where(d => d.IsIndexed == true)
            .Include(d => d.Subject)
            .ToListAsync();
}

