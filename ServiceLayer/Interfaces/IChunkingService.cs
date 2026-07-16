namespace ServiceLayer.Interfaces;

public interface IChunkingService
{
    /// <summary>
    /// Tách văn bản thành các đoạn (chunk) dựa trên cấu hình lấy từ Database (System Config).
    /// Hỗ trợ chia theo: Đoạn văn (Paragraphs), Từ (Words) hoặc Ký tự (Characters).
    /// </summary>
    /// <param name="text">Toàn bộ văn bản cần tách</param>
    Task<List<string>> ChunkTextAsync(string text);
}
