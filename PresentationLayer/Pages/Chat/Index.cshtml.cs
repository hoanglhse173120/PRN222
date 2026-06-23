using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceLayer.Interfaces;

namespace PresentationLayer.Pages.Chat;

[Authorize(Roles = "Admin,Student")]
public class IndexModel : PageModel
{
    private readonly IChatService _chatService;
    private readonly UserManager<IdentityUser> _userManager;

    public IndexModel(IChatService chatService, UserManager<IdentityUser> userManager)
    {
        _chatService = chatService;
        _userManager = userManager;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = _userManager.GetUserId(User) ?? "";
        var sessions = await _chatService.GetAllSessionsByUserAsync(userId);
        var latest = sessions.OrderByDescending(s => s.CreatedAt).FirstOrDefault();

        if (latest != null)
            return RedirectToPage("Session", new { id = latest.SessionID });

        var newSession = await _chatService.CreateSessionAsync(userId, "Phiên chat mới");
        return RedirectToPage("Session", new { id = newSession.SessionID });
    }
}
