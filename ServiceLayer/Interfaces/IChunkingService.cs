namespace ServiceLayer.Interfaces;

public interface IChunkingService
{
    /// <summary>
    /// Tách văn bản thành các đoạn (chunk) theo số từ.
    /// </summary>
    /// <param name="text">Toàn bộ văn bản cần tách</param>
    /// <param name="chunkSize">Số từ mỗi chunk (mặc định 500)</param>
    /// <param name="overlap">Số từ chồng lấp giữa 2 chunk liền kề (mặc định 50)</param>
    List<string> ChunkByWords(string text, int chunkSize = 500, int overlap = 50);
}
