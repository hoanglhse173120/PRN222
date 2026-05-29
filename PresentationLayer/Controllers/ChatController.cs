using ServiceLayer.DTOs;
using Microsoft.AspNetCore.Mvc;
using PresentationLayer.ViewModels;
using ServiceLayer.Interfaces;

namespace PresentationLayer.Controllers;

public class ChatController : Controller
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
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
        var session = await _chatService.CreateSessionAsync(string.IsNullOrWhiteSpace(sessionName) ? "New Chat" : sessionName);
        return RedirectToAction(nameof(Session), new { id = session.SessionID });
    }

    // GET: /Chat/Session/5
    public async Task<IActionResult> Session(int id)
    {
        var session = await _chatService.GetSessionWithMessagesAsync(id);
        if (session == null) return NotFound();

        var vm = new ChatSessionViewModel
        {
            Session = session
        };
        return View(vm);
    }

    // POST: /Chat/SendMessage
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendMessage(int sessionId, string newMessage)
    {
        if (string.IsNullOrWhiteSpace(newMessage))
        {
            return RedirectToAction(nameof(Session), new { id = sessionId });
        }

        // 1. Lưu tin nhắn của user
        await _chatService.AddMessageAsync(sessionId, "user", newMessage);

        // 2. TODO: Gọi RAG Service để lấy câu trả lời từ AI
        // Tạm thời mock câu trả lời của AI
        string aiReply = $"[MOCK] Tôi là chatbot. Bạn vừa hỏi: '{newMessage}'. Do chưa nối model AI nên tôi trả lời tạm thế này nhé.";
        await _chatService.AddMessageAsync(sessionId, "assistant", aiReply);

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
