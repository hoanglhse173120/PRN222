using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using PresentationLayer.Hubs;
using ServiceLayer.DTOs;
using ServiceLayer.Interfaces;

namespace PresentationLayer.Pages.Subject;

[Authorize(Roles = "Admin")]
public class EditModel : PageModel
{
    private readonly ISubjectService _subjectService;
    private readonly IHubContext<ChatHub> _hubContext;
    public EditModel(ISubjectService subjectService, IHubContext<ChatHub> hubContext)
    {
        _subjectService = subjectService;
        _hubContext = hubContext;
    }

    [BindProperty]
    public SubjectDto Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var subject = await _subjectService.GetByIdAsync(id);
        if (subject == null) return NotFound();
        Input = subject;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        await _subjectService.UpdateAsync(Input);
        await _hubContext.Clients.All.SendAsync("SubjectChanged", new
        {
            action      = "updated",
            subjectId   = Input.SubjectID,
            subjectName = Input.SubjectName
        });
        TempData["Success"] = "Cập nhật môn học thành công!";
        return RedirectToPage("Index");
    }
}
