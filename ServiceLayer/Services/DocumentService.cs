using DataAccessLayer.Models;
using DataAccessLayer.Repositories;
using ServiceLayer.Interfaces;
using System.Text.Json;
using System.Net.Http.Json;
using System.Net.Http;

namespace ServiceLayer.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _repo;
    private readonly IRepository<DocumentChunk> _chunkRepo;
    private readonly ITextExtractorService _extractor;
    private readonly IChunkingService _chunker;
    private readonly IHttpClientFactory _httpClientFactory;

    public DocumentService(
        IDocumentRepository repo,
        IRepository<DocumentChunk> chunkRepo,
        ITextExtractorService extractor,
        IChunkingService chunker,
        IHttpClientFactory httpClientFactory)
    {
        _repo = repo;
        _chunkRepo = chunkRepo;
        _extractor = extractor;
        _chunker = chunker;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IEnumerable<Document>> GetAllAsync() => await _repo.GetAllAsync();

    public async Task<IEnumerable<Document>> GetBySubjectAsync(int subjectId)
        => await _repo.GetBySubjectAsync(subjectId);

    public async Task<Document?> GetByIdAsync(int id) => await _repo.GetByIdAsync(id);

    public async Task<Document> UploadAsync(int subjectId, string fileName, string fileType, string filePath, long fileSizeKB)
    {
        var doc = new Document
        {
            SubjectID = subjectId,
            FileName = fileName,
            FileType = fileType,
            FilePath = filePath,
            FileSizeKB = fileSizeKB,
            IsIndexed = false,
            UploadedAt = DateTime.Now
        };
        await _repo.AddAsync(doc);
        await _repo.SaveChangesAsync();
        return doc;
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
            _repo.Delete(doc);
            await _repo.SaveChangesAsync();
        }
    }

    public async Task<int> IndexDocumentAsync(int documentId, string webRootPath, int chunkSize = 500, int overlap = 50)
    {
        var doc = await _repo.GetByIdAsync(documentId)
            ?? throw new InvalidOperationException($"Không tìm thấy tài liệu ID={documentId}");

        if (doc.IsIndexed)
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
                DocumentID = documentId,
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
