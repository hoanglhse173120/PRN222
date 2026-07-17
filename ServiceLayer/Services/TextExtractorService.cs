using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using ServiceLayer.Interfaces;
using UglyToad.PdfPig;

namespace ServiceLayer.Services;

public class TextExtractorService : ITextExtractorService
{
    public async Task<string> ExtractTextAsync(string physicalFilePath, string fileType)
    {
        if (!File.Exists(physicalFilePath))
            throw new FileNotFoundException($"Không tìm thấy file: {physicalFilePath}");

        return fileType.ToUpper() switch
        {
            "PDF"        => ExtractFromPdf(physicalFilePath),
            "DOCX"       => ExtractFromDocx(physicalFilePath),
            "DOC"        => ExtractFromDocx(physicalFilePath),
            "PPTX"       => ExtractFromPptx(physicalFilePath),
            "PPT"        => ExtractFromPptx(physicalFilePath),
            _            => await File.ReadAllTextAsync(physicalFilePath)
        };
    }

    // ── PDF ────────────────────────────────────────────────────────────────
    private static string ExtractFromPdf(string filePath)
    {
        var sb = new StringBuilder();
        using var pdf = PdfDocument.Open(filePath);
        foreach (var page in pdf.GetPages())
        {
            var words = page.GetWords();
            sb.AppendLine(string.Join(" ", words.Select(w => w.Text)));
        }
        return sb.ToString();
    }

    // ── DOCX ───────────────────────────────────────────────────────────────
    private static string ExtractFromDocx(string filePath)
    {
        var sb = new StringBuilder();
        using var doc = WordprocessingDocument.Open(filePath, false);
        var body = doc.MainDocumentPart?.Document?.Body;
        if (body == null) return string.Empty;

        foreach (var para in body.Descendants<Paragraph>())
        {
            var line = para.InnerText.Trim();
            if (!string.IsNullOrWhiteSpace(line))
                sb.AppendLine(line);
        }
        return sb.ToString();
    }

    // ── PPTX ───────────────────────────────────────────────────────────────
    private static string ExtractFromPptx(string filePath)
    {
        var sb = new StringBuilder();
        using var pres = PresentationDocument.Open(filePath, false);
        var slideParts = pres.PresentationPart?.SlideParts;
        if (slideParts == null) return string.Empty;

        int slideNum = 1;
        foreach (var slidePart in slideParts)
        {
            sb.AppendLine($"--- Slide {slideNum++} ---");
            var descendants = slidePart.Slide?.Descendants<DocumentFormat.OpenXml.Drawing.Text>();
            if (descendants != null)
            {
                foreach (var textElem in descendants)
                {
                    var text = textElem.Text?.Trim();
                    if (!string.IsNullOrWhiteSpace(text))
                        sb.AppendLine(text);
                }
            }
        }
        return sb.ToString();
    }
}
