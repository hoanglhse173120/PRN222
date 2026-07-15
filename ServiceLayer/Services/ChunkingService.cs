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

    private List<string> ChunkByParagraphs(string text, int maxSize, int overlap)
    {
        var paragraphs = text.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries)
                             .Select(p => p.Trim())
                             .Where(p => !string.IsNullOrEmpty(p))
                             .ToArray();

        var chunks = new List<string>();
        int step = Math.Max(1, maxSize - overlap);
        int i = 0;

        while (i < paragraphs.Length)
        {
            var chunkParagraphs = paragraphs.Skip(i).Take(maxSize).ToArray();
            var chunkText = string.Join("\n\n", chunkParagraphs).Trim();

            if (!string.IsNullOrWhiteSpace(chunkText))
                chunks.Add(chunkText);

            if (i + maxSize >= paragraphs.Length)
                break;

            i += step;
        }

        return chunks;
    }
}
