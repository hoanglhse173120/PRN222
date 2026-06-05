using DataAccessLayer.Entities;
using DataAccessLayer.Repositories;
using ServiceLayer.Interfaces;
using ServiceLayer.DTOs;
using System.Text.Json;
using System.Net.Http.Json;
using System.Net.Http;
using System.Linq;
using DataAccessLayer.Context;
using Microsoft.EntityFrameworkCore;

namespace ServiceLayer.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _repo;
    private readonly IRepository<DocumentChunk> _chunkRepo;
    private readonly ITextExtractorService _extractor;
    private readonly IChunkingService _chunker;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ChatbotDbContext _db;

    public DocumentService(
        IDocumentRepository repo,
        IRepository<DocumentChunk> chunkRepo,
        ITextExtractorService extractor,
        IChunkingService chunker,
        IHttpClientFactory httpClientFactory,
        ChatbotDbContext db)
    {
        _repo = repo;
        _chunkRepo = chunkRepo;
        _extractor = extractor;
        _chunker = chunker;
        _httpClientFactory = httpClientFactory;
        _db = db;
    }

    public async Task<IEnumerable<DocumentDto>> GetAllAsync()
    {
        var entities = await _repo.GetAllAsync();
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<DocumentDto>> GetBySubjectAsync(int subjectId)
    {
        var entities = await _repo.GetBySubjectAsync(subjectId);
        return entities.Select(MapToDto);
    }

    public async Task<DocumentDto?> GetByIdAsync(int id)
    {
        var doc = await _repo.GetByIdAsync(id);
        if (doc == null) return null;
        return MapToDto(doc);
    }

    public async Task<DocumentDto> UploadAsync(int subjectId, string fileName, string fileType, string filePath, long? fileSizeKB)
    {
        var doc = new Document
        {
            SubjectId = subjectId,
            FileName = fileName,
            FileType = fileType,
            FilePath = filePath,
            FileSizeKb = fileSizeKB,
            IsIndexed = false,
            UploadedAt = DateTime.Now
        };
        await _repo.AddAsync(doc);
        await _repo.SaveChangesAsync();
        return MapToDto(doc);
    }

    private DocumentDto MapToDto(Document doc)
    {
        return new DocumentDto
        {
            DocumentID = doc.DocumentId,
            SubjectID = doc.SubjectId,
            FileName = doc.FileName,
            FileType = doc.FileType,
            FilePath = doc.FilePath,
            FileSizeKB = doc.FileSizeKb,
            IsIndexed = doc.IsIndexed ?? false,
            UploadedAt = doc.UploadedAt ?? DateTime.MinValue,
            Subject = doc.Subject != null ? new SubjectDto { SubjectID = doc.Subject.SubjectId, SubjectName = doc.Subject.SubjectName } : null
        };
    }

    public async Task<DocumentDetailsDto?> GetDetailsWithChunksAsync(int id)
    {
        var doc = await _repo.GetByIdWithChunksAsync(id);
        if (doc == null) return null;

        return new DocumentDetailsDto
        {
            DocumentID = doc.DocumentId,
            FileName = doc.FileName,
            FileType = doc.FileType,
            FilePath = doc.FilePath,
            FileSizeKB = doc.FileSizeKb,
            IsIndexed = doc.IsIndexed ?? false,
            UploadedAt = doc.UploadedAt ?? DateTime.MinValue,
            Subject = doc.Subject != null
                ? new SubjectDto { SubjectID = doc.Subject.SubjectId, SubjectName = doc.Subject.SubjectName }
                : null,
            Chunks = doc.DocumentChunks.Select(c => new DocumentChunkDto
            {
                ChunkID = c.ChunkId,
                DocumentID = c.DocumentId,
                ChunkIndex = c.ChunkIndex,
                ChunkContent = c.ChunkContent,
                CreatedAt = c.CreatedAt ?? DateTime.MinValue,
                EmbeddingJson = c.Embedding
            }).ToList()
        };
    }

    public async Task MarkAsIndexedAsync(int documentId)
    {
        var doc = await _repo.GetByIdAsync(documentId);
        if (doc != null)
        {
            doc.IsIndexed = true;
            _repo.Update(doc);
            await _repo.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(int id)
    {
        var doc = await _repo.GetByIdAsync(id);
        if (doc != null)
        {
            // TÌM VÀ XÓA THỦ CÔNG CÁC MessageSource LIÊN QUAN ĐẾN CHUNKS CỦA TÀI LIỆU NÀY
            // Để tránh lỗi "The DELETE statement conflicted with the REFERENCE constraint..." 
            // do MessageSource -> DocumentChunk là OnDelete(NoAction).
            var chunkIds = await _db.DocumentChunks
                .Where(c => c.DocumentId == id)
                .Select(c => c.ChunkId)
                .ToListAsync();

            if (chunkIds.Any())
            {
                // Thực thi trực tiếp xuống DB để chắc chắn xoá xong MessageSource trước khi EF xoá Document
                await _db.MessageSources
                    .Where(ms => chunkIds.Contains(ms.ChunkId))
                    .ExecuteDeleteAsync();
            }

            _repo.Delete(doc);
            await _repo.SaveChangesAsync(); 
        }
    }

    public async Task<int> IndexDocumentAsync(int documentId, string webRootPath, int chunkSize = 500, int overlap = 50)
    {
        var doc = await _repo.GetByIdAsync(documentId)
            ?? throw new InvalidOperationException($"Không tìm thấy tài liệu ID={documentId}");

        if (doc.IsIndexed == true)
            return 0; // đã index rồi, bỏ qua

        // Tính đường dẫn vật lý từ đường dẫn tương đối lưu trong DB
        // FilePath lưu dạng: /uploads/1/guid_filename.pdf
        var physicalPath = Path.Combine(webRootPath, doc.FilePath!.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

        // Bước 1: Extract text
        var rawText = await _extractor.ExtractTextAsync(physicalPath, doc.FileType ?? "txt");

        if (string.IsNullOrWhiteSpace(rawText))
            throw new InvalidOperationException("Không trích xuất được nội dung từ file.");

        // Bước 2: Chunk
        var chunks = _chunker.ChunkByWords(rawText, chunkSize, overlap);

        // Bước 3: Lấy Embeddings từ Python API (e5-base)
        var client = _httpClientFactory.CreateClient();
        var requestPayload = new
        {
            texts = chunks,
            prefix = "passage: "
        };

        var response = await client.PostAsJsonAsync("http://127.0.0.1:8000/embed", requestPayload);
        response.EnsureSuccessStatusCode();

        var responseData = await response.Content.ReadFromJsonAsync<EmbedResponse>();
        if (responseData == null || responseData.Embeddings == null || responseData.Embeddings.Count != chunks.Count)
        {
            throw new InvalidOperationException("Lỗi khi lấy vector embeddings từ Python service.");
        }

        // Bước 4: Lưu từng chunk (kèm vector) vào DocumentChunks
        for (int i = 0; i < chunks.Count; i++)
        {
            var chunk = new DocumentChunk
            {
                DocumentId = documentId,
                ChunkContent = chunks[i],
                ChunkIndex = i + 1,
                Embedding = JsonSerializer.Serialize(responseData.Embeddings[i]),
                CreatedAt = DateTime.Now
            };
            await _chunkRepo.AddAsync(chunk);
        }

        // Bước 5: Đánh dấu đã index
        doc.IsIndexed = true;
        _repo.Update(doc);
        await _repo.SaveChangesAsync();

        return chunks.Count; // trả về số chunk đã tạo
    }

    private class EmbedResponse
    {
        public List<List<float>> Embeddings { get; set; } = new();
    }
}
