using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceLayer.DTOs;
using ServiceLayer.Interfaces;

namespace PresentationLayer.Pages.Subject;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly ISubjectService _subjectService;
    public IndexModel(ISubjectService subjectService) => _subjectService = subjectService;

    public IEnumerable<SubjectDto> Subjects { get; set; } = Enumerable.Empty<SubjectDto>();

    public async Task OnGetAsync()
    {
        Subjects = await _subjectService.GetAllAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        await _subjectService.DeleteAsync(id);
        TempData["Success"] = "Đã xóa môn học.";
        return RedirectToPage();
    }
}
