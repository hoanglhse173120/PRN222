using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using ServiceLayer.Interfaces;
using System.Collections.Concurrent;

namespace PresentationLayer.Hubs;

/// <summary>
/// SignalR Hub chịu trách nhiệm giao tiếp thời gian thực (Real-time) cho tính năng Chatbot (RAG).
/// Nhận tin nhắn từ client, đẩy cho thuật toán RAG xử lý và stream câu trả lời liên tục (chunk) về lại cho người dùng.
/// Đồng thời xử lý các tác vụ như lưu tin nhắn vào DB, đặt tên session.
/// </summary>
[Authorize(Roles = "Admin,Student,Teacher")]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly IRagService _ragService;
    private static readonly ConcurrentDictionary<string, CancellationTokenSource> _activeGenerations = new();

    public ChatHub(IChatService chatService, IRagService ragService)
    {
        _chatService = chatService;
        _ragService = ragService;
    }

    /// <summary>
    /// Được gọi từ Client khi người dùng gửi tin nhắn thông qua WebSocket (SignalR).
    /// Hàm này đảm nhiệm việc lưu lịch sử tin nhắn, tự động sinh tiêu đề (nếu là tin đầu tiên),
    /// gọi Pipeline RAG để truy xuất thông tin và đẩy (stream) câu trả lời về lại Client để tạo trải nghiệm sinh động.
    /// </summary>
    public async Task SendMessage(int sessionId, string message)
    {
        var userId = Context.UserIdentifier ?? "";

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
            newSessionName = await _ragService.GenerateTitleAsync(message);
            await _chatService.RenameSessionAsync(sessionId, userId, newSessionName);
        }

        // 4. Báo client đã nhận tin nhắn user (để hiển thị bubble)
        await Clients.Caller.SendAsync("ReceiveUserMessage", message, newSessionName);

        var cts = new CancellationTokenSource();
        _activeGenerations[Context.ConnectionId] = cts;
        var partialAnswer = new System.Text.StringBuilder();

        try
        {
            // 5. Lấy lịch sử hội thoại (bỏ tin vừa lưu để tránh trùng)
            var session = await _chatService.GetSessionWithMessagesAsync(sessionId, userId);
            var history = session?.ChatMessages
                .OrderBy(m => m.Timestamp)
                .SkipLast(1)
                .ToList();

            // 6. Gọi RAG pipeline
            var ragResult = await _ragService.AskAsync(message, history, session?.SubjectId, 5, async (chunk) => 
            {
                partialAnswer.Append(chunk);
                // Mã hóa chuỗi chunk trước khi stream về để chống XSS
                var encodedChunk = System.Net.WebUtility.HtmlEncode(chunk);
                await Clients.Caller.SendAsync("StreamNext", encodedChunk);
            }, cts.Token);
            var rawAnswer = ragResult.Answer;

            // 7. Lưu câu trả lời vào DB
            await _chatService.AddMessageWithSourcesAsync(sessionId, "assistant", rawAnswer, ragResult.Sources);

            // 8. Render markdown đơn giản (An toàn XSS: Encode trước khi parse Markdown)
            var encodedAnswer = System.Net.WebUtility.HtmlEncode(rawAnswer);
            var rendered = System.Text.RegularExpressions.Regex
                .Replace(encodedAnswer, @"\*\*(.*?)\*\*", "<strong>$1</strong>")
                .Replace("\n", "<br/>");

            // 9. Push kết quả về caller
            await Clients.Caller.SendAsync("StreamComplete", rendered, newSessionName, ragResult.Sources);
        }
        catch (OperationCanceledException)
        {
            var rawAnswer = partialAnswer.ToString();
            var encodedAnswer = System.Net.WebUtility.HtmlEncode(rawAnswer);
            var rendered = System.Text.RegularExpressions.Regex
                .Replace(encodedAnswer, @"\*\*(.*?)\*\*", "<strong>$1</strong>")
                .Replace("\n", "<br/>");
            
            await _chatService.AddMessageAsync(sessionId, "assistant", rawAnswer);
            await Clients.Caller.SendAsync("StreamComplete", rendered, newSessionName, new List<ServiceLayer.DTOs.RagChunkResultDto>());
        }
        catch (Exception ex)
        {
            var errMsg = ex.InnerException?.Message ?? ex.Message;
            var errText = $"⚠️ Lỗi khi xử lý câu hỏi: {errMsg}\n\nĐảm bảo Python AI Service đang chạy ở cổng 8000 và đã có tài liệu được index.";
            await _chatService.AddMessageAsync(sessionId, "assistant", errText);

            var rendered = errText.Replace("\n", "<br/>");
            await Clients.Caller.SendAsync("StreamComplete", rendered, newSessionName);
        }
        finally
        {
            _activeGenerations.TryRemove(Context.ConnectionId, out _);
        }
    }

    /// <summary>
    /// Kích hoạt cờ huỷ (CancellationToken) để tạm dừng quá trình AI sinh văn bản đang thực thi (nếu có).
    /// Cải thiện trải nghiệm người dùng khi AI sinh lan man hoặc người dùng thấy đủ kết quả.
    /// </summary>
    public void StopGenerating()
    {
        if (_activeGenerations.TryGetValue(Context.ConnectionId, out var cts))
        {
            cts.Cancel();
        }
    }
}
