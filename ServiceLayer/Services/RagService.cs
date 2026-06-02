using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using DataAccessLayer.Repositories;
using Microsoft.Extensions.Configuration;
using ServiceLayer.DTOs;
using ServiceLayer.Interfaces;

namespace ServiceLayer.Services;

public class RagService : IRagService
{
    private readonly IDocumentRepository _docRepo;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;

    public RagService(
        IDocumentRepository docRepo,
        IHttpClientFactory httpClientFactory,
        IConfiguration config)
    {
        _docRepo = docRepo;
        _httpClientFactory = httpClientFactory;
        _config = config;
    }

    public async Task<RagResponseDto> AskAsync(string question, int topK = 5)
    {
        topK = int.TryParse(_config["Rag:TopK"], out var cfgK) ? cfgK : topK;
        var minScore = double.TryParse(_config["Rag:MinScore"], out var ms) ? ms : 0.35;
        var embedUrl = _config["Rag:EmbedApiUrl"] ?? "http://127.0.0.1:8000/embed";

        // ── Step 1: Embed the query ─────────────────────────────────────────
        var queryVector = await EmbedQueryAsync(question, embedUrl);

        // ── Step 2: Load all indexed chunks and compute cosine similarity ───
        var allChunks = (await _docRepo.GetAllIndexedChunksAsync()).ToList();

        if (!allChunks.Any())
        {
            return new RagResponseDto
            {
                Answer = "Chưa có tài liệu nào được index trong hệ thống. Vui lòng upload và index tài liệu trước.",
                IsFromDocuments = false,
                Sources = new()
            };
        }

        var scoredChunks = allChunks
            .Where(c => !string.IsNullOrEmpty(c.Embedding))
            .Select(c =>
            {
                var chunkVec = JsonSerializer.Deserialize<float[]>(c.Embedding!)!;
                var score = CosineSimilarity(queryVector, chunkVec);
                return (chunk: c, score);
            })
            .Where(x => x.score >= minScore)
            .OrderByDescending(x => x.score)
            .Take(topK)
            .ToList();

        if (!scoredChunks.Any())
        {
            return new RagResponseDto
            {
                Answer = "Tôi không tìm thấy thông tin liên quan trong tài liệu đã index. Vui lòng hỏi về nội dung có trong tài liệu.",
                IsFromDocuments = false,
                Sources = new()
            };
        }

        // ── Step 3: Build context and call Gemini ───────────────────────────
        var contextBuilder = new StringBuilder();
        contextBuilder.AppendLine("Dưới đây là các đoạn trích từ tài liệu liên quan đến câu hỏi:");
        contextBuilder.AppendLine();
        for (int i = 0; i < scoredChunks.Count; i++)
        {
            var (chunk, score) = scoredChunks[i];
            contextBuilder.AppendLine($"[Đoạn {i + 1}] Tài liệu: {chunk.Document.FileName} (Môn: {chunk.Document.Subject?.SubjectName ?? "N/A"})");
            contextBuilder.AppendLine(chunk.ChunkContent);
            contextBuilder.AppendLine();
        }

        var answer = await CallLlmAsync(question, contextBuilder.ToString());

        var sources = scoredChunks.Select(x => new RagChunkResultDto
        {
            ChunkID = x.chunk.ChunkID,
            DocumentID = x.chunk.DocumentID,
            FileName = x.chunk.Document.FileName,
            SubjectName = x.chunk.Document.Subject?.SubjectName,
            ChunkContent = x.chunk.ChunkContent.Length > 300
                ? x.chunk.ChunkContent[..300] + "..."
                : x.chunk.ChunkContent,
            ChunkIndex = x.chunk.ChunkIndex,
            Score = Math.Round(x.score, 4)
        }).ToList();

        return new RagResponseDto
        {
            Answer = answer,
            Sources = sources,
            IsFromDocuments = true
        };
    }

    // ── Cosine Similarity ───────────────────────────────────────────────────
    private static double CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length != b.Length) return 0;
        double dot = 0, normA = 0, normB = 0;
        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }
        return normA == 0 || normB == 0 ? 0 : dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
    }

    // ── Embed query via Python API ─────────────────────────────────────────
    private async Task<float[]> EmbedQueryAsync(string query, string embedUrl)
    {
        var client = _httpClientFactory.CreateClient();
        var payload = new { texts = new[] { query }, prefix = "query: " };
        var response = await client.PostAsJsonAsync(embedUrl, payload);
        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadFromJsonAsync<EmbedApiResponse>();
        if (data?.Embeddings == null || data.Embeddings.Count == 0)
            throw new InvalidOperationException("Python embed API không trả về kết quả.");

        return data.Embeddings[0].ToArray();
    }

    // ── Call Groq REST API ───────────────────────────────────────────────
    private async Task<string> CallLlmAsync(string question, string context)
    {
        var apiKey = _config["Groq:ApiKey"];
        var model = _config["Groq:Model"] ?? "llama3-70b-8192";

        if (string.IsNullOrWhiteSpace(apiKey))
            return "[Cấu hình] Chưa cài API key Groq. Vui lòng thêm Groq:ApiKey vào appsettings.json.";

        var requestBody = new
        {
            model = model,
            messages = new[]
            {
                new { role = "system", content = "Bạn là trợ lý học tập thông minh. Chỉ trả lời dựa trên ngữ cảnh tài liệu được cung cấp. Nếu câu hỏi không liên quan đến nội dung tài liệu, hãy nói rõ điều đó. Trả lời bằng tiếng Việt, ngắn gọn, chính xác và dễ hiểu." },
                new { role = "user", content = $"Ngữ cảnh:\n{context}\n\nCâu hỏi: {question}" }
            },
            temperature = 0.3,
            max_tokens = 1024
        };

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
        
        var url = "https://api.groq.com/openai/v1/chat/completions";
        var response = await client.PostAsJsonAsync(url, requestBody);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            return $"[Groq API lỗi {response.StatusCode}]: {err[..Math.Min(200, err.Length)]}";
        }

        var result = await response.Content.ReadFromJsonAsync<GroqResponse>();
        return result?.Choices?.FirstOrDefault()?.Message?.Content
               ?? "AI không trả về câu trả lời.";
    }

    // ── Private response models ────────────────────────────────────────────
    private class EmbedApiResponse
    {
        public List<List<float>> Embeddings { get; set; } = new();
    }

    private class GroqResponse
    {
        public List<GroqChoice>? Choices { get; set; }
    }
    private class GroqChoice
    {
        public GroqMessage? Message { get; set; }
    }
    private class GroqMessage
    {
        public string? Content { get; set; }
    }
}
