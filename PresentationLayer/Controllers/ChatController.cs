using Microsoft.AspNetCore.Authorization;
using ServiceLayer.DTOs;
using Microsoft.AspNetCore.Mvc;
using PresentationLayer.ViewModels;
using ServiceLayer.Interfaces;

namespace PresentationLayer.Controllers;

[Authorize(Roles = "Student")]
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

        // POST: /Chat/SendMessageAjax  — RAG Pipeline cho AJAX (Không làm khựng web)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessageAjax(int sessionId, string newMessage)
        {
            if (string.IsNullOrWhiteSpace(newMessage))
                return Json(new { success = false, message = "Empty message" });

            // 1. Lưu câu hỏi của user
            await _chatService.AddMessageAsync(sessionId, "user", newMessage);

            string rawAnswer = "";
            try
            {
                // 2. Gọi RAG pipeline: embed → search → Gemini/Groq
                var ragResult = await _ragService.AskAsync(newMessage);

                // 3. Lấy câu trả lời
                rawAnswer = ragResult.Answer;

                // 4. Lưu câu trả lời của AI (vẫn lưu nguồn tham khảo MessageSource vào database)
                await _chatService.AddMessageWithSourcesAsync(
                    sessionId, "assistant", rawAnswer, ragResult.Sources);
            }
            catch (Exception ex)
            {
                // Nếu Python service chưa chạy hoặc lỗi khác
                var errMsg = ex.InnerException?.Message ?? ex.Message;
                rawAnswer = $"⚠️ Lỗi khi xử lý câu hỏi: {errMsg}\n\nĐảm bảo Python AI Service đang chạy ở cổng 8000 và đã có tài liệu được index.";
                
                await _chatService.AddMessageAsync(sessionId, "assistant", rawAnswer);
            }

            // Xử lý text (để HTML hiển thị đẹp các dấu in đậm và xuống dòng)
            var rendered = System.Text.RegularExpressions.Regex
                .Replace(rawAnswer, @"\*\*(.*?)\*\*", "<strong>$1</strong>")
                .Replace("\n", "<br/>");

            return Json(new { success = true, answerHtml = rendered });
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
