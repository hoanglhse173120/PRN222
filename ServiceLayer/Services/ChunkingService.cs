using ServiceLayer.Interfaces;
using DataAccessLayer.Entities;
using DataAccessLayer.Repositories;

namespace ServiceLayer.Services;

public class ChunkingService : IChunkingService
{
    private readonly IRepository<ChunkingConfig> _configRepo;

    public ChunkingService(IRepository<ChunkingConfig> configRepo)
    {
        _configRepo = configRepo;
    }

    /// <summary>
    /// Thực hiện chia nhỏ văn bản (chunking) dựa trên cấu hình được lưu trong CSDL.
    /// Nếu chưa có cấu hình trong DB, hệ thống sẽ tự động dùng cấu hình mặc định (Words, MaxSize 500, Overlap 50).
    /// </summary>
    /// <param name="text">Đoạn văn bản gốc cần được chia nhỏ</param>
    /// <returns>Danh sách các đoạn văn bản (chunks) đã được chia nhỏ</returns>
    public async Task<List<string>> ChunkTextAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new List<string>();

        // Lấy config từ DB (singleton record thường có Id = 1)
        var configs = await _configRepo.GetAllAsync();
        var config = configs.FirstOrDefault() ?? new ChunkingConfig { Strategy = "Words", MaxSize = 500, Overlap = 50 };

        return config.Strategy.ToLower() switch
        {
            "paragraphs" => ChunkByParagraphs(text, config.MaxSize, config.Overlap),
            "characters" => ChunkByCharacters(text, config.MaxSize, config.Overlap),
            _ => ChunkByWords(text, config.MaxSize, config.Overlap)
        };
    }

    /// <summary>
    /// Thuật toán chia nhỏ văn bản theo số lượng từ (Words).
    /// Phân tách từ dựa trên khoảng trắng, xuống dòng, dấu tab.
    /// </summary>
    /// <param name="text">Văn bản cần chia</param>
    /// <param name="maxSize">Số lượng từ tối đa phép chứa trong một chunk</param>
    /// <param name="overlap">Số từ lặp lại giữa 2 chunk liên tục nhằm bảo toàn ngữ cảnh</param>
    /// <returns>Danh sách các chunk đã tạo</returns>
    private List<string> ChunkByWords(string text, int maxSize, int overlap)
    {
        var words = text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        var chunks = new List<string>();
        int step = Math.Max(1, maxSize - overlap);
        int i = 0;

        while (i < words.Length)
        {
            var chunkWords = words.Skip(i).Take(maxSize).ToArray();
            var chunkText = string.Join(' ', chunkWords).Trim();

            if (!string.IsNullOrWhiteSpace(chunkText))
                chunks.Add(chunkText);

            if (i + maxSize >= words.Length)
                break;

            i += step;
        }

        return chunks;
    }

    /// <summary>
    /// Thuật toán chia nhỏ theo ký tự (Characters).
    /// Lấy điệp khúc chính xác theo độ dài ký tự bất chấp là khoảng trắng hay chữ, có thể cắt ngang một từ.
    /// </summary>
    /// <param name="text">Văn bản cần chia</param>
    /// <param name="maxSize">Số lượng ký tự tối đa trong một chunk</param>
    /// <param name="overlap">Số ký tự giao nhau giữa 2 chunk kề nhau</param>
    /// <returns>Danh sách các chunk</returns>
    private List<string> ChunkByCharacters(string text, int maxSize, int overlap)
    {
        var chunks = new List<string>();
        int step = Math.Max(1, maxSize - overlap);
        int i = 0;

        while (i < text.Length)
        {
            int length = Math.Min(maxSize, text.Length - i);
            var chunkText = text.Substring(i, length).Trim();

            if (!string.IsNullOrWhiteSpace(chunkText))
                chunks.Add(chunkText);

            if (i + maxSize >= text.Length)
                break;

            i += step;
        }

        return chunks;
    }

    /// <summary>
    /// Thuật toán chia nhỏ văn bản theo từng đoạn văn (Paragraphs).
    /// Các đoạn văn được nhận dạng và phân tách thông qua ký tự ngắt dòng (\r\n hoặc \n).
    /// Phương pháp này giúp giữ nguyên cấu trúc logic, mạch lạc của tài liệu một cách xuất sắc.
    /// </summary>
    /// <param name="text">Văn bản cần chia</param>
    /// <param name="maxSize">Số lượng đoạn văn tối đa gộp vào một chunk</param>
    /// <param name="overlap">Số đoạn văn lặp lại ở điểm tiếp nối giữa các chunk</param>
    /// <returns>Danh sách các chunk</returns>
    private List<string> ChunkByParagraphs(string text, int maxSize, int overlap)
    {
        var paragraphs = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                             .Select(p => p.Trim())
                             .Where(p => !string.IsNullOrEmpty(p))
                             .ToArray();

        var chunks = new List<string>();
        int step = Math.Max(1, maxSize - overlap);
        int i = 0;

        while (i < paragraphs.Length)
        {
            var chunkParagraphs = paragraphs.Skip(i).Take(maxSize).ToArray();
            var chunkText = string.Join("\n", chunkParagraphs).Trim();

            if (!string.IsNullOrWhiteSpace(chunkText))
                chunks.Add(chunkText);

            if (i + maxSize >= paragraphs.Length)
                break;

            i += step;
        }

        return chunks;
    }
}
