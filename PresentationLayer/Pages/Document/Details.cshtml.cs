using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceLayer.DTOs;
using ServiceLayer.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace PresentationLayer.Pages.Document;

[Authorize(Roles = "Teacher,Student,Admin")]
public class DetailsModel : PageModel
{
    private readonly IDocumentService _documentService;
    private readonly ISubjectService _subjectService;
    private readonly UserManager<IdentityUser> _userManager;
    
    public DetailsModel(
        IDocumentService documentService,
        ISubjectService subjectService,
        UserManager<IdentityUser> userManager)
    {
        _documentService = documentService;
        _subjectService = subjectService;
        _userManager = userManager;
    }

    public DocumentDetailsDto DocDetails { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var details = await _documentService.GetDetailsWithChunksAsync(id);
        if (details == null) return NotFound();

        if (User.IsInRole("Teacher"))
        {
            var userId = _userManager.GetUserId(User) ?? "";
            var assignedIds = await _subjectService.GetAssignedSubjectIdsAsync(userId);
            
            if (details.Subject == null || !assignedIds.Contains(details.Subject.SubjectID))
            {
                TempData["Error"] = "Bạn không có quyền xem chi tiết tài liệu của môn học này.";
                return RedirectToPage("Index");
            }
        }

        DocDetails = details;
        return Page();
    }
}
