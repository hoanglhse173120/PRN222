using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceLayer.DTOs;
using ServiceLayer.Interfaces;

namespace PresentationLayer.Pages.Chat;

[Authorize(Roles = "Admin,Student")]
public class SessionModel : PageModel
{
    private readonly IChatService _chatService;
    private readonly UserManager<IdentityUser> _userManager;

    public SessionModel(IChatService chatService, UserManager<IdentityUser> userManager)
    {
        _chatService = chatService;
        _userManager = userManager;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public ChatSessionDto Session { get; set; } = null!;
    public IList<ChatSessionDto> AllSessions { get; set; } = new List<ChatSessionDto>();

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = _userManager.GetUserId(User) ?? "";
        var session = await _chatService.GetSessionWithMessagesAsync(Id, userId);
        if (session == null) return NotFound();

        Session = session;
        AllSessions = (await _chatService.GetAllSessionsByUserAsync(userId)).ToList();
        return Page();
    }

    // POST: create new session
    public async Task<IActionResult> OnPostCreateAsync(string? sessionName)
    {
        var userId = _userManager.GetUserId(User) ?? "";
        var session = await _chatService.CreateSessionAsync(
            userId, string.IsNullOrWhiteSpace(sessionName) ? "Phiên chat mới" : sessionName);
        return RedirectToPage(new { id = session.SessionID });
    }

    // POST: delete session
    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var userId = _userManager.GetUserId(User) ?? "";
        await _chatService.DeleteSessionAsync(id, userId);
        TempData["Success"] = "Đã xóa phiên trò chuyện.";
        return RedirectToPage("/Chat/Index");
    }
}
