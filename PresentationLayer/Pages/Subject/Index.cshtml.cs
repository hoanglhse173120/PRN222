using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using PresentationLayer.Hubs;
using ServiceLayer.DTOs;
using ServiceLayer.Interfaces;

namespace PresentationLayer.Pages.Subject;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly ISubjectService _subjectService;
    private readonly IHubContext<ChatHub> _hubContext;
    public IndexModel(ISubjectService subjectService, IHubContext<ChatHub> hubContext)
    {
        _subjectService = subjectService;
        _hubContext = hubContext;
    }

    public IEnumerable<SubjectDto> Subjects { get; set; } = Enumerable.Empty<SubjectDto>();

    public async Task OnGetAsync()
    {
        Subjects = await _subjectService.GetAllAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        await _subjectService.DeleteAsync(id);
        await _hubContext.Clients.All.SendAsync("SubjectChanged", new
        {
            action    = "deleted",
            subjectId = id
        });
        TempData["Success"] = "Đã xóa môn học.";
        return RedirectToPage();
    }
}
