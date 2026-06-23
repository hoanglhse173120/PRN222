using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using ServiceLayer.Interfaces;

namespace PresentationLayer.Hubs;

[Authorize(Roles = "Admin,Student")]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly IRagService _ragService;
    private readonly UserManager<IdentityUser> _userManager;

    public ChatHub(IChatService chatService, IRagService ragService, UserManager<IdentityUser> userManager)
    {
        _chatService = chatService;
        _ragService = ragService;
        _userManager = userManager;
    }

    /// <summary>
    /// Được gọi từ client khi người dùng gửi tin nhắn.
    /// Hub lưu tin, gọi RAG pipeline, rồi push kết quả về client qua WebSocket.
    /// </summary>
    public async Task SendMessage(int sessionId, string message)
    {
        var userId = _userManager.GetUserId(Context.User) ?? "";

        if (string.IsNullOrWhiteSpace(message))
        {
            await Clients.Caller.SendAsync("StreamError", "Tin nhắn không được để trống.");
            return;
        }

        // 1. Kiểm tra session hợp lệ & thuộc user
        var sessionBefore = await _chatService.GetSessionWithMessagesAsync(sessionId, userId);
        if (sessionBefore == null)
        {
            await Clients.Caller.SendAsync("StreamError", "Không tìm thấy phiên chat.");
            return;
        }

        bool isFirstMessage = sessionBefore.ChatMessages.Count == 0;

        // 2. Lưu câu hỏi user
        await _chatService.AddMessageAsync(sessionId, "user", message);

        // 3. Tự động đặt tên session từ tin nhắn đầu tiên
        string? newSessionName = null;
        if (isFirstMessage)
        {
            newSessionName = message.Length > 40
                ? message[..40].TrimEnd() + "…"
                : message;
            await _chatService.RenameSessionAsync(sessionId, userId, newSessionName);
        }

        // 4. Báo client đã nhận tin nhắn user (để hiển thị bubble)
        await Clients.Caller.SendAsync("ReceiveUserMessage", message, newSessionName);

        try
        {
            // 5. Lấy lịch sử hội thoại (bỏ tin vừa lưu để tránh trùng)
            var session = await _chatService.GetSessionWithMessagesAsync(sessionId, userId);
            var history = session?.ChatMessages
                .OrderBy(m => m.Timestamp)
                .SkipLast(1)
                .ToList();

            // 6. Gọi RAG pipeline
            var ragResult = await _ragService.AskAsync(message, history, 5, async (chunk) => 
            {
                // Push từng mảnh text về client qua SignalR
                await Clients.Caller.SendAsync("StreamNext", chunk);
            });
            var rawAnswer = ragResult.Answer;

            // 7. Lưu câu trả lời vào DB
            await _chatService.AddMessageWithSourcesAsync(sessionId, "assistant", rawAnswer, ragResult.Sources);

            // 8. Render markdown đơn giản
            var rendered = System.Text.RegularExpressions.Regex
                .Replace(rawAnswer, @"\*\*(.*?)\*\*", "<strong>$1</strong>")
                .Replace("\n", "<br/>");

            // 9. Push kết quả về caller
            await Clients.Caller.SendAsync("StreamComplete", rendered, newSessionName, ragResult.Sources);
        }
        catch (Exception ex)
        {
            var errMsg = ex.InnerException?.Message ?? ex.Message;
            var errText = $"⚠️ Lỗi khi xử lý câu hỏi: {errMsg}\n\nĐảm bảo Python AI Service đang chạy ở cổng 8000 và đã có tài liệu được index.";
            await _chatService.AddMessageAsync(sessionId, "assistant", errText);

            var rendered = errText.Replace("\n", "<br/>");
            await Clients.Caller.SendAsync("StreamComplete", rendered, newSessionName);
        }
    }
}
