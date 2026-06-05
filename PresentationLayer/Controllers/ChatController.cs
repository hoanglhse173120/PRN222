using Microsoft.AspNetCore.Authorization;
using ServiceLayer.DTOs;
using Microsoft.AspNetCore.Mvc;
using PresentationLayer.ViewModels;
using ServiceLayer.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace PresentationLayer.Controllers;

[Authorize(Roles = "Admin,Student")]
public class ChatController : Controller
{
    private readonly IChatService _chatService;
    private readonly IRagService _ragService;
    private readonly UserManager<IdentityUser> _userManager;

    public ChatController(IChatService chatService, IRagService ragService, UserManager<IdentityUser> userManager)
    {
        _chatService = chatService;
        _ragService = ragService;
        _userManager = userManager;
    }

    // GET: /Chat  — redirect thẳng vào session mới nhất hoặc tạo mới
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User) ?? "";
        var sessions = await _chatService.GetAllSessionsByUserAsync(userId);
        var latest = sessions.OrderByDescending(s => s.CreatedAt).FirstOrDefault();

        if (latest != null)
            return RedirectToAction(nameof(Session), new { id = latest.SessionID });

        // Chưa có session nào → tạo mới luôn
        var newSession = await _chatService.CreateSessionAsync(userId, "Phiên chat mới");
        return RedirectToAction(nameof(Session), new { id = newSession.SessionID });
    }

    // POST: /Chat/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string? sessionName)
    {
        var userId = _userManager.GetUserId(User) ?? "";
        var session = await _chatService.CreateSessionAsync(
            userId, string.IsNullOrWhiteSpace(sessionName) ? "Phiên chat mới" : sessionName);
        return RedirectToAction(nameof(Session), new { id = session.SessionID });
    }

    // GET: /Chat/Session/5
    public async Task<IActionResult> Session(int id)
    {
        var userId = _userManager.GetUserId(User) ?? "";
        var session = await _chatService.GetSessionWithMessagesAsync(id, userId);
        if (session == null) return NotFound();

        var allSessions = await _chatService.GetAllSessionsByUserAsync(userId);

        var vm = new ChatSessionViewModel
        {
            Session = session,
            AllSessions = allSessions
        };
        return View(vm);
    }

        // POST: /Chat/SendMessageAjax  — RAG Pipeline cho AJAX
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessageAjax(int sessionId, string newMessage)
        {
            var userId = _userManager.GetUserId(User) ?? "";
            if (string.IsNullOrWhiteSpace(newMessage))
                return Json(new { success = false, message = "Empty message" });

            // 1. Lấy session để kiểm tra có phải tin nhắn đầu không
            var sessionBefore = await _chatService.GetSessionWithMessagesAsync(sessionId, userId);
            if (sessionBefore == null) return Unauthorized();
            bool isFirstMessage = sessionBefore.ChatMessages.Count == 0;

            // 2. Lưu câu hỏi của user
            await _chatService.AddMessageAsync(sessionId, "user", newMessage);

            // 3. Tự động đặt tên session từ tin nhắn đầu tiên
            string? newSessionName = null;
            if (isFirstMessage)
            {
                newSessionName = newMessage.Length > 40
                    ? newMessage[..40].TrimEnd() + "…"
                    : newMessage;
                await _chatService.RenameSessionAsync(sessionId, userId, newSessionName);
            }

            string rawAnswer = "";
            try
            {
                // 4. Lấy lịch sử hội thoại (trừ câu hỏi vừa lưu để tránh trùng)
                var session = await _chatService.GetSessionWithMessagesAsync(sessionId, userId);
                var history = session?.ChatMessages
                    .OrderBy(m => m.Timestamp)
                    .SkipLast(1)
                    .ToList();

                // 5. Gọi RAG pipeline với lịch sử hội thoại (multi-turn)
                var ragResult = await _ragService.AskAsync(newMessage, history);
                rawAnswer = ragResult.Answer;

                // 6. Lưu câu trả lời của AI
                await _chatService.AddMessageWithSourcesAsync(
                    sessionId, "assistant", rawAnswer, ragResult.Sources);
            }
            catch (Exception ex)
            {
                var errMsg = ex.InnerException?.Message ?? ex.Message;
                rawAnswer = $"⚠️ Lỗi khi xử lý câu hỏi: {errMsg}\n\nĐảm bảo Python AI Service đang chạy ở cổng 8000 và đã có tài liệu được index.";
                await _chatService.AddMessageAsync(sessionId, "assistant", rawAnswer);
            }

            var rendered = System.Text.RegularExpressions.Regex
                .Replace(rawAnswer, @"\*\*(.*?)\*\*", "<strong>$1</strong>")
                .Replace("\n", "<br/>");

            return Json(new { success = true, answerHtml = rendered, sessionName = newSessionName });
        }

        // POST: /Chat/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = _userManager.GetUserId(User) ?? "";
        await _chatService.DeleteSessionAsync(id, userId);
        TempData["Success"] = "Đã xóa phiên trò chuyện.";
        return RedirectToAction(nameof(Index));
    }
}
