using ServiceLayer.DTOs;
using Microsoft.AspNetCore.Mvc;
using PresentationLayer.ViewModels;
using ServiceLayer.Interfaces;

namespace PresentationLayer.Controllers;

public class ChatController : Controller
{
    private readonly IChatService _chatService;
    private readonly IRagService _ragService;

    public ChatController(IChatService chatService, IRagService ragService)
    {
        _chatService = chatService;
        _ragService = ragService;
    }

    // GET: /Chat
    public async Task<IActionResult> Index()
    {
        var sessions = await _chatService.GetAllSessionsAsync();
        return View(sessions);
    }

    // POST: /Chat/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string? sessionName)
    {
        var session = await _chatService.CreateSessionAsync(
            string.IsNullOrWhiteSpace(sessionName) ? "Phiên chat mới" : sessionName);
        return RedirectToAction(nameof(Session), new { id = session.SessionID });
    }

    // GET: /Chat/Session/5
    public async Task<IActionResult> Session(int id)
    {
        var session = await _chatService.GetSessionWithMessagesAsync(id);
        if (session == null) return NotFound();

        var vm = new ChatSessionViewModel { Session = session };
        return View(vm);
    }

    // POST: /Chat/SendMessage  — RAG Pipeline
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendMessage(int sessionId, string newMessage)
    {
        if (string.IsNullOrWhiteSpace(newMessage))
            return RedirectToAction(nameof(Session), new { id = sessionId });

        // 1. Lưu câu hỏi của user
        await _chatService.AddMessageAsync(sessionId, "user", newMessage);

        try
        {
            // 2. Gọi RAG pipeline: embed → search → Gemini
            var ragResult = await _ragService.AskAsync(newMessage);

            // 3. Format answer kèm sources
            var answerText = ragResult.Answer;
            if (ragResult.IsFromDocuments && ragResult.Sources.Any())
            {
                answerText += "\n\n📚 **Nguồn tham khảo:**\n" +
                    string.Join("\n", ragResult.Sources.Select((s, i) =>
                        $"{i + 1}. [{s.FileName}] Chunk #{s.ChunkIndex} (score: {s.Score:F2})"));
            }

            // 4. Lưu câu trả lời của AI (kèm MessageSource citations)
            await _chatService.AddMessageWithSourcesAsync(
                sessionId, "assistant", answerText, ragResult.Sources);
        }
        catch (Exception ex)
        {
            // Nếu Python service chưa chạy hoặc lỗi khác
            var errMsg = ex.InnerException?.Message ?? ex.Message;
            await _chatService.AddMessageAsync(sessionId, "assistant",
                $"⚠️ Lỗi khi xử lý câu hỏi: {errMsg}\n\n" +
                "Đảm bảo Python AI Service đang chạy ở cổng 8000 và đã có tài liệu được index.");
        }

        return RedirectToAction(nameof(Session), new { id = sessionId });
    }

    // POST: /Chat/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _chatService.DeleteSessionAsync(id);
        TempData["Success"] = "Đã xóa phiên trò chuyện.";
        return RedirectToAction(nameof(Index));
    }
}
