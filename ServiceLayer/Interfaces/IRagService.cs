using ServiceLayer.DTOs;

namespace ServiceLayer.Interfaces;

public interface IRagService
{
    /// <summary>
    /// Nhận câu hỏi của user, tìm chunks liên quan qua vector similarity,
    /// gọi LLM tạo câu trả lời có ngữ cảnh hội thoại trước đó (multi-turn).
    /// </summary>
    Task<RagResponseDto> AskAsync(
        string question,
        IEnumerable<ChatMessageDto>? conversationHistory = null,
        int topK = 5);
}
