using ServiceLayer.DTOs;

namespace ServiceLayer.Interfaces;

public interface IRagService
{
    /// <summary>
    /// Nhận câu hỏi của user, tìm chunks liên quan qua vector similarity,
    /// gọi LLM tạo câu trả lời có trích dẫn nguồn.
    /// </summary>
    Task<RagResponseDto> AskAsync(string question, int topK = 5);
}
