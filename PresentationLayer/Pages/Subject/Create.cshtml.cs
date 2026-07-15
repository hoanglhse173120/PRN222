using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using PresentationLayer.Hubs;
using ServiceLayer.DTOs;
using ServiceLayer.Interfaces;

namespace PresentationLayer.Pages.Subject;

[Authorize(Roles = "Admin")]
public class CreateModel : PageModel
{
    private readonly ISubjectService _subjectService;
    private readonly IHubContext<ChatHub> _hubContext;
    public CreateModel(ISubjectService subjectService, IHubContext<ChatHub> hubContext)
    {
        _subjectService = subjectService;
        _hubContext = hubContext;
    }

    [BindProperty]
    public SubjectDto Input { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        await _subjectService.CreateAsync(Input);
        await _hubContext.Clients.All.SendAsync("SubjectChanged", new
        {
            action      = "created",
            subjectName = Input.SubjectName
        });
        TempData["Success"] = $"Đã tạo môn học \"{Input.SubjectName}\" thành công!";
        return RedirectToPage("Index");
    }
}
