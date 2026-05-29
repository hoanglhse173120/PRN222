namespace ServiceLayer.Interfaces;

public interface ITextExtractorService
{
    /// <summary>Trích xuất toàn bộ text từ file, trả về chuỗi văn bản thuần.</summary>
    Task<string> ExtractTextAsync(string physicalFilePath, string fileType);
}
