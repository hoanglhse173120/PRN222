using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceLayer.DTOs;
using ServiceLayer.Interfaces;

namespace PresentationLayer.Pages.Subject;

[Authorize(Roles = "Admin")]
public class CreateModel : PageModel
{
    private readonly ISubjectService _subjectService;
    public CreateModel(ISubjectService subjectService) => _subjectService = subjectService;

    [BindProperty]
    public SubjectDto Input { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        await _subjectService.CreateAsync(Input);
        TempData["Success"] = $"Đã tạo môn học \"{Input.SubjectName}\" thành công!";
        return RedirectToPage("Index");
    }
}
