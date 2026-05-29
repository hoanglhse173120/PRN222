using ServiceLayer.Interfaces;

namespace ServiceLayer.Services;

public class ChunkingService : IChunkingService
{
    /// <summary>
    /// Tách văn bản thành các chunks theo số từ, có overlap để giữ ngữ cảnh giữa 2 chunk liền kề.
    /// 
    /// Ví dụ với chunkSize=500, overlap=50:
    ///   Chunk 1: từ 0   → 499
    ///   Chunk 2: từ 450 → 949  (chồng 50 từ với chunk 1)
    ///   Chunk 3: từ 900 → 1399 (chồng 50 từ với chunk 2)
    /// </summary>
    public List<string> ChunkByWords(string text, int chunkSize = 500, int overlap = 50)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new List<string>();

        // Tách thành mảng từ, bỏ khoảng trắng thừa
        var words = text
            .Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);

        var chunks = new List<string>();
        int step = Math.Max(1, chunkSize - overlap); // bước nhảy giữa 2 chunk
        int i = 0;

        while (i < words.Length)
        {
            // Lấy tối đa chunkSize từ, bắt đầu từ vị trí i
            var chunkWords = words.Skip(i).Take(chunkSize).ToArray();
            var chunkText = string.Join(' ', chunkWords).Trim();

            if (!string.IsNullOrWhiteSpace(chunkText))
                chunks.Add(chunkText);

            // Nếu đã lấy hết từ còn lại → dừng
            if (i + chunkSize >= words.Length)
                break;

            i += step;
        }

        return chunks;
    }
}
