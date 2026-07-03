using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using DataAccessLayer.Repositories;
using Microsoft.Extensions.Configuration;
using ServiceLayer.DTOs;
using ServiceLayer.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace ServiceLayer.Services;

/// <summary>
/// Dịch vụ cốt lõi xử lý luồng (pipeline) của mô hình RAG (Retrieval-Augmented Generation).
/// Cung cấp các hàm nhúng câu hỏi (Embedding), tìm kiếm đoạn tài liệu tương đồng nhất 
/// trong Database và tích hợp sinh văn bản hội thoại (LLM - Groq).
/// </summary>
public class RagService : IRagService
{
    private readonly IDocumentRepository _docRepo;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;

    public RagService(
        IDocumentRepository docRepo,
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        IMemoryCache cache)
    {
        _docRepo = docRepo;
        _httpClientFactory = httpClientFactory;
        _config = config;
        _cache = cache;
    }

    /// <summary>
    /// Xử lý toàn diện câu hỏi của người dùng, thực thi 3 bước của RAG:
    /// Bước 1: Gọi embed model để mã hoá câu hỏi đầu vào thành Vector.
    /// Bước 2: Truy xuất Database lấy Vectors, tính độ gần gũi bằng Cosine Similarity để lọc topK chunk khớp.
    /// Bước 3: Ráp chunk (đóng gói ngữ cảnh) + prompt, gọi LLM qua Groq API lấy câu trả lời và luồng (stream) kết quả về.
    /// </summary>
    /// <param name="question">Nội dung câu hỏi của người dùng</param>
    /// <param name="conversationHistory">Chuỗi các tin nhắn trước đó (lịch sử trò chuyện cho multi-turn chat)</param>
    /// <param name="subjectId">Chỉ định tìm không gian con theo môn học (tăng tốc độ và giảm nhiễu), null khi muốn tìm toàn cục</param>
    /// <param name="topK">Giới hạn số lượng chunks tốt nhất được mượn làm tri thức (context)</param>
    /// <param name="onChunkReceived">Callback đón nhận Real-time chữ của AI khi đang nói</param>
    /// <param name="cancellationToken">Cờ huỷ khi người dùng nhấn StopGenerating</param>
    /// <returns>Đối tượng đóng gói đáp án toàn vẹn và thông tin trích dẫn nguồn</returns>
    public async Task<RagResponseDto> AskAsync(
        string question,
        IEnumerable<ChatMessageDto>? conversationHistory = null,
        int? subjectId = null,
        int topK = 5,
        Func<string, Task>? onChunkReceived = null,
        CancellationToken cancellationToken = default)
    {
        topK = int.TryParse(_config["Rag:TopK"], out var cfgK) ? cfgK : topK;
        var minScore = double.TryParse(_config["Rag:MinScore"], out var ms) ? ms : 0.35;
        var embedUrl = _config["Rag:EmbedApiUrl"] ?? "http://127.0.0.1:8000/embed";

        // ── Step 1: Embed the query ─────────────────────────────────────────
        var queryVector = await EmbedQueryAsync(question, embedUrl);

        // ── Step 2: Load all indexed chunks and compute cosine similarity ───
        var cacheKey = subjectId.HasValue ? $"RagChunks_Subject_{subjectId}" : "RagChunks_Subject_All";
        if (!_cache.TryGetValue(cacheKey, out List<RagCacheItemDto>? allChunks))
        {
            var entities = await _docRepo.GetAllIndexedChunksAsync(subjectId);
            allChunks = entities.Select(c => new RagCacheItemDto
            {
                ChunkId = c.ChunkId,
                DocumentId = c.DocumentId,
                ChunkIndex = c.ChunkIndex,
                ChunkContent = c.ChunkContent,
                Embedding = c.Embedding,
                SubjectId = c.Document.SubjectId,
                SubjectName = c.Document.Subject?.SubjectName,
                FileName = c.Document.FileName ?? "Unknown",
                ParsedEmbedding = string.IsNullOrEmpty(c.Embedding) ? null : JsonSerializer.Deserialize<float[]>(c.Embedding)
            }).ToList();

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                .SetAbsoluteExpiration(TimeSpan.FromHours(6));

            _cache.Set(cacheKey, allChunks, cacheEntryOptions);
        }

        if (allChunks == null || !allChunks.Any())
        {
            return new RagResponseDto
            {
                Answer = "Chưa có tài liệu nào được index trong hệ thống. Vui lòng upload và index tài liệu trước.",
                IsFromDocuments = false,
                Sources = new()
            };
        }

        var scoredChunks = allChunks
            .Where(c => c.ParsedEmbedding != null)
            .Select(c =>
            {
                var score = CosineSimilarity(queryVector, c.ParsedEmbedding!);
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
            contextBuilder.AppendLine($"[Đoạn {i + 1}] Tài liệu: {chunk.FileName} (Môn: {chunk.SubjectName ?? "N/A"})");
            contextBuilder.AppendLine(chunk.ChunkContent);
            contextBuilder.AppendLine();
        }

        var answer = await CallLlmAsync(question, contextBuilder.ToString(), conversationHistory, onChunkReceived, cancellationToken);

        var sources = scoredChunks.Select(x => new RagChunkResultDto
        {
            ChunkID = x.chunk.ChunkId,
            DocumentID = x.chunk.DocumentId,
            FileName = x.chunk.FileName,
            SubjectName = x.chunk.SubjectName,
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
    /// <summary>
    /// Thuật toán tìm kiếm độ tương đồng Cosine Similarity giữa 2 vector.
    /// Bằng thương của tích vô hướng trên tích độ dài.
    /// Trả về số thập phân từ -1 đến 1 (Càng gần 1, hai đoạn text càng mang nghĩa giống nhau).
    /// </summary>
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
    /// <summary>
    /// Gọi microservice Python (FastAPI/HuggingFace) để nhúng câu văn thành vector embeddings (biểu diễn số toán học).
    /// </summary>
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

    // ── Call Groq REST API (multi-turn) ─────────────────────────────────────
    /// <summary>
    /// Tích hợp Model ngôn ngữ lớn (LLM - LLama3 / Gemini qua API Groq).
    /// Ráp các chunk tìm được làm "system instruction context" đảm bảo BOT không bị Halucination (bịa chuyện).
    /// Hỗ trợ luồng SSE (Server-Sent Events) để trả dữ liệu theo Stream mượt mà.
    /// </summary>
    private async Task<string> CallLlmAsync(
        string question,
        string context,
        IEnumerable<ChatMessageDto>? conversationHistory = null,
        Func<string, Task>? onChunkReceived = null,
        CancellationToken cancellationToken = default)
    {
        var apiKey = _config["Groq:ApiKey"];
        var model = _config["Groq:Model"] ?? "llama3-70b-8192";

        if (string.IsNullOrWhiteSpace(apiKey))
            return "[Cấu hình] Chưa cài API key Groq. Vui lòng thêm Groq:ApiKey vào appsettings.json.";

        // ── Xây dựng mảng messages cho LLM (multi-turn) ─────────────────────
        var systemPrompt = "Bạn là trợ lý học tập. Chỉ trả lời dựa trên tài liệu được cung cấp. " +
            "Nếu câu trả lời không có trong 'Ngữ cảnh tài liệu' được cung cấp bên dưới, hãy trả lời chính xác là 'Tôi không tìm thấy thông tin này trong tài liệu môn học' và không tự bịa ra câu trả lời. " +
            "BẮT BUỘC 100% dùng tiếng Việt (chữ Quốc ngữ). Nghiêm cấm dùng chữ Hán (Chinese characters) như 模, 隐藏, 暴. " +
            "Đối với thuật ngữ chuyên ngành (như Modularity, Encapsulation...), hãy giữ nguyên tiếng Anh hoặc dịch sang tiếng Việt thuần thục " +
            "(ví dụ: tính mô-đun hóa, tính đóng gói). " +
            "Bạn có thể nhớ lịch sử các tin nhắn trong cuộc hội thoại để trả lời theo ngữ cảnh.";

        var messages = new List<object>
        {
            new { role = "system", content = systemPrompt }
        };

        // Thêm lịch sử hội thoại trước đó (tối đa 10 tin nhắn, không vượt quá 3000 ký tự)
        if (conversationHistory != null)
        {
            int currentHistoryLength = 0;
            var historyToAdd = new List<object>();

            foreach (var msg in conversationHistory.OrderByDescending(m => m.Timestamp).Take(10))
            {
                if (currentHistoryLength + msg.MessageText.Length > 3000)
                    break;

                var historyRole = msg.Role == "user" ? "user" : "assistant";
                historyToAdd.Insert(0, new { role = historyRole, content = msg.MessageText });
                currentHistoryLength += msg.MessageText.Length;
            }

            messages.AddRange(historyToAdd);
        }

        // Câu hỏi hiện tại kèm ngữ cảnh tài liệu (RAG context)
        messages.Add(new
        {
            role = "user",
            content = $"Ngữ cảnh tài liệu:\n{context}\n\nCâu hỏi hiện tại: {question}"
        });

        var requestBody = new
        {
            model,
            messages = messages.ToArray(),
            temperature = 0.3,
            max_tokens = 1024,
            stream = onChunkReceived != null
        };

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

        var url = "https://api.groq.com/openai/v1/chat/completions";
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(requestBody)
        };

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(cancellationToken);
            return $"[Groq API lỗi {response.StatusCode}]: {err[..Math.Min(200, err.Length)]}";
        }

        if (onChunkReceived == null)
        {
            var result = await response.Content.ReadFromJsonAsync<GroqResponse>(cancellationToken: cancellationToken);
            return result?.Choices?.FirstOrDefault()?.Message?.Content ?? "AI không trả về câu trả lời.";
        }

        // Streaming (SSE)
        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new System.IO.StreamReader(stream);
        var fullAnswerBuilder = new StringBuilder();

        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (line.StartsWith("data: "))
            {
                var data = line.Substring(6);
                if (data == "[DONE]") break;

                try
                {
                    var chunkObj = JsonSerializer.Deserialize<GroqStreamResponse>(data, jsonOptions);
                    var content = chunkObj?.Choices?.FirstOrDefault()?.Delta?.Content;
                    if (!string.IsNullOrEmpty(content))
                    {
                        fullAnswerBuilder.Append(content);
                        await onChunkReceived(content);
                    }
                }
                catch { /* Bỏ qua lỗi parse của các dòng không hợp lệ */ }
            }
        }

        return fullAnswerBuilder.ToString();
    }

    /// <summary>
    /// Tự động sinh tên chủ đề cực kỳ ngắn gọn cho đoạn chat dựa vào lời mở đầu của User.
    /// Chức năng AI tóm tắt.
    /// </summary>
    public async Task<string> GenerateTitleAsync(string firstMessage)
    {
        var apiKey = _config["Groq:ApiKey"];
        var model = _config["Groq:Model"] ?? "llama3-70b-8192";
        if (string.IsNullOrWhiteSpace(apiKey)) return firstMessage.Length > 40 ? firstMessage[..40] + "…" : firstMessage;

        var messages = new List<object>
        {
            new { role = "system", content = "Bạn là AI đặt tên. Dựa vào nội dung người dùng gửi, hãy tóm tắt và đặt tên cho cuộc hội thoại này bằng Tiếng Việt. CHỈ TRẢ VỀ TÊN (khoảng 3-6 từ), KHÔNG CẦN GIẢI THÍCH HAY BỎ TRONG NGOẶC KÉP." },
            new { role = "user", content = firstMessage }
        };

        var requestBody = new
        {
            model,
            messages = messages.ToArray(),
            temperature = 0.5,
            max_tokens = 20
        };

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
        
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions")
        {
            Content = JsonContent.Create(requestBody)
        };

        try {
            using var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode) return firstMessage.Length > 40 ? firstMessage[..40] + "…" : firstMessage;
            var result = await response.Content.ReadFromJsonAsync<GroqResponse>();
            return result?.Choices?.FirstOrDefault()?.Message?.Content?.Trim('"', '\'', ' ', '\n') ?? firstMessage;
        } catch {
            return firstMessage.Length > 40 ? firstMessage[..40] + "…" : firstMessage;
        }
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

    private class GroqStreamResponse
    {
        public List<GroqStreamChoice>? Choices { get; set; }
    }
    private class GroqStreamChoice
    {
        public GroqDelta? Delta { get; set; }
    }
    private class GroqDelta
    {
        public string? Content { get; set; }
    }
}
