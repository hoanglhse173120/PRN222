using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceLayer.DTOs;
using ServiceLayer.Interfaces;
using Microsoft.AspNetCore.SignalR;
using PresentationLayer.Hubs;

namespace PresentationLayer.Pages.Chat;

[Authorize(Roles = "Admin,Student")]
public class IndexModel : PageModel
{
    private readonly IChatService _chatService;
    private readonly ISubjectService _subjectService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IHubContext<ChatHub> _hubContext;

    public IndexModel(IChatService chatService, ISubjectService subjectService, UserManager<IdentityUser> userManager, IHubContext<ChatHub> hubContext)
    {
        _chatService = chatService;
        _subjectService = subjectService;
        _userManager = userManager;
        _hubContext = hubContext;
    }

    public IList<SubjectDto> Subjects { get; set; } = new List<SubjectDto>();
    public IList<ChatSessionDto> RecentSessions { get; set; } = new List<ChatSessionDto>();

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = _userManager.GetUserId(User) ?? "";
        
        // Lấy danh sách môn học để hiển thị
        Subjects = (await _subjectService.GetAllAsync()).ToList();

        // Lấy lịch sử chat
        RecentSessions = (await _chatService.GetAllSessionsByUserAsync(userId)).Take(5).ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostStartChatAsync(int? subjectId)
    {
        var userId = _userManager.GetUserId(User) ?? "";
        var newSession = await _chatService.CreateSessionAsync(userId, subjectId, "Phiên chat mới");
        await _hubContext.Clients.All.SendAsync("NewChatSessionCreated");
        return RedirectToPage("Session", new { id = newSession.SessionID });
    }
}
