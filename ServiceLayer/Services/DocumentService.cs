using DataAccessLayer.Entities;
using DataAccessLayer.Repositories;
using ServiceLayer.Interfaces;
using ServiceLayer.DTOs;
using System.Text.Json;
using System.Net.Http.Json;
using System.Net.Http;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;

namespace ServiceLayer.Services;

/// <summary>
/// Service thực thi các nghiệp vụ cốt lõi về tài liệu:
/// Upload, truy xuất thông tin, xử lý file và đặc biệt là Index (tiền xử lý) tài liệu để phục vụ kiến trúc RAG.
/// </summary>
public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _repo;
    private readonly IRepository<DocumentChunk> _chunkRepo;
    private readonly ITextExtractorService _extractor;
    private readonly IChunkingService _chunker;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;

    public DocumentService(
        IDocumentRepository repo,
        IRepository<DocumentChunk> chunkRepo,
        ITextExtractorService extractor,
        IChunkingService chunker,
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache)
    {
        _repo = repo;
        _chunkRepo = chunkRepo;
        _extractor = extractor;
        _chunker = chunker;
        _httpClientFactory = httpClientFactory;
        _cache = cache;
    }

    /// <summary>
    /// Lấy danh sách toàn bộ các tài liệu đang có trong hệ thống (Mapping sang Dto gọn nhẹ).
    /// </summary>
    public async Task<IEnumerable<DocumentDto>> GetAllAsync()
    {
        var entities = await _repo.GetAllAsync();
        return entities.Select(MapToDto);
    }

    /// <summary>
    /// Truy xuất danh sách tài liệu được phân quyền theo một môn học (Subject) nhất định.
    /// </summary>
    public async Task<IEnumerable<DocumentDto>> GetBySubjectAsync(int subjectId)
    {
        var entities = await _repo.GetBySubjectAsync(subjectId);
        return entities.Select(MapToDto);
    }

    /// <summary>
    /// Đọc thông tin cơ bản Metadata của một tài liệu dựa theo Khóa chính (Id).
    /// </summary>
    public async Task<DocumentDto?> GetByIdAsync(int id)
    {
        var doc = await _repo.GetByIdAsync(id);
        if (doc == null) return null;
        return MapToDto(doc);
    }

    /// <summary>
    /// Lưu thông tin tài liệu mới (metadata của file) vào cơ sở dữ liệu sau khi được upload.
    /// Trạng thái IsIndexed ban đầu luôn là false.
    /// </summary>
    public async Task<DocumentDto> UploadAsync(int subjectId, string fileName, string fileType, string filePath, long? fileSizeKB, string userId)
    {
        var doc = new Document
        {
            SubjectId = subjectId,
            FileName = fileName,
            FileType = fileType,
            FilePath = filePath,
            FileSizeKb = fileSizeKB,
            IsIndexed = false,
            UploadedAt = DateTime.Now,
            UploadedByUserId = userId
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
            UploaderName = doc.UploadedByUser?.UserName ?? doc.UploadedByUser?.Email ?? "N/A",
            Subject = doc.Subject != null ? new SubjectDto { SubjectID = doc.Subject.SubjectId, SubjectName = doc.Subject.SubjectName } : null
        };
    }

    /// <summary>
    /// Lấy thông tin chi tiết một tài liệu cùng với tất cả các đoạn cắt (Chunks) nội dung và
    /// Vector Embeddings của tài liệu đó. Phục vụ việc xem trước (Preview) tài liệu đã index.
    /// </summary>
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
            UploaderName = doc.UploadedByUser?.UserName ?? doc.UploadedByUser?.Email ?? "N/A",
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

    /// <summary>
    /// Xóa hoàn toàn một tài liệu gồm: Record tài liệu, các thẻ Meta/Chucks, các tin nhắn tham chiếu nguồn, 
    /// Clear system cache và cuối cùng là xoá file vật lý ở ổ cứng.
    /// </summary>
    public async Task DeleteAsync(int id, string webRootPath)
    {
        var doc = await _repo.GetByIdAsync(id);
        if (doc != null)
        {
            // Xóa Cache của RAG
            _cache.Remove("RagChunks_Subject_All");
            _cache.Remove($"RagChunks_Subject_{doc.SubjectId}");

            // Xác định đường dẫn file vật lý
            string? physicalPath = null;
            if (!string.IsNullOrWhiteSpace(doc.FilePath))
            {
                physicalPath = Path.Combine(webRootPath, doc.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            }

            // TÌM VÀ XÓA CÁC MessageSource LIÊN QUAN ĐẾN CHUNKS CỦA TÀI LIỆU NÀY thông qua DAL
            await _repo.DeleteMessageSourcesByDocumentIdAsync(id);

            _repo.Delete(doc);
            
            // Lưu database trước
            await _repo.SaveChangesAsync(); 
            
            // Xóa file vật lý SAU KHI save database thành công
            if (physicalPath != null && System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
            }
        }
    }

    /// <summary>
    /// Trái tim của quá trình Ingestion (Tiền xử lý) trong hệ thống RAG:
    /// 1. Đọc và trích xuất nguyên bản text từ file (PDF, Docx, TXT).
    /// 2. Cắt nhỏ (Chunk) text để tối ưu cho mô hình ngôn ngữ vừa và nhỏ.
    /// 3. Gọi model mã hoá (EmbedService) qua Python API để quy đổi chuỗi text thành Vector.
    /// 4. Lưu lại các Chunk kèm vector (Embedding) tương đối vào Database để sẵn sàng đem so sánh Cosine Similarity.
    /// </summary>
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

        // Xóa Cache của RAG
        _cache.Remove("RagChunks_Subject_All");
        _cache.Remove($"RagChunks_Subject_{doc.SubjectId}");

        return chunks.Count; // trả về số chunk đã tạo
    }

    private class EmbedResponse
    {
        public List<List<float>> Embeddings { get; set; } = new();
    }
}
