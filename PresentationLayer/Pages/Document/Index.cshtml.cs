using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using PresentationLayer.Hubs;
using PresentationLayer.ViewModels;
using ServiceLayer.Interfaces;

namespace PresentationLayer.Pages.Document;

[Authorize(Roles = "Teacher,Student,Admin")]
public class IndexModel : PageModel
{
    private readonly IDocumentService _documentService;
    private readonly ISubjectService _subjectService;
    private readonly IWebHostEnvironment _env;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IHubContext<ChatHub> _hubContext;

    public IndexModel(
        IDocumentService documentService, 
        ISubjectService subjectService,
        IWebHostEnvironment env,
        UserManager<IdentityUser> userManager,
        IHubContext<ChatHub> hubContext)
    {
        _documentService = documentService;
        _subjectService = subjectService;
        _env = env;
        _userManager = userManager;
        _hubContext = hubContext;
    }

    [BindProperty(SupportsGet = true)]
    public int? SubjectId { get; set; }

    public DocumentIndexViewModel Vm { get; set; } = null!;

    public async Task OnGetAsync()
    {
        var subjects = await _subjectService.GetAllAsync();
        var documents = SubjectId.HasValue
            ? await _documentService.GetBySubjectAsync(SubjectId.Value)
            : await _documentService.GetAllAsync();

        if (User.IsInRole("Teacher"))
        {
            var userId = _userManager.GetUserId(User) ?? "";
            var assignedIds = await _subjectService.GetAssignedSubjectIdsAsync(userId);
            
            subjects = subjects.Where(s => assignedIds.Contains(s.SubjectID));
            documents = documents.Where(d => assignedIds.Contains(d.SubjectID));
        }

        Vm = new DocumentIndexViewModel
        {
            Documents = documents,
            Subjects = subjects,
            SelectedSubjectId = SubjectId,
        };
    }


    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        if (User.IsInRole("Student") || User.IsInRole("Admin"))
        {
            TempData["Error"] = "Bạn không có quyền xoá tài liệu.";
            return RedirectToPage();
        }

        if (User.IsInRole("Teacher"))
        {
            var doc = await _documentService.GetAllAsync();
            var target = doc.FirstOrDefault(d => d.DocumentID == id);
            if (target == null) return NotFound();

            var userId = _userManager.GetUserId(User) ?? "";
            var assignedIds = await _subjectService.GetAssignedSubjectIdsAsync(userId);

            if (!assignedIds.Contains(target.SubjectID))
            {
                TempData["Error"] = "Bạn chỉ được xóa tài liệu thuộc môn học được phân công.";
                return RedirectToPage();
            }
        }

        await _documentService.DeleteAsync(id, _env.WebRootPath);

        await _hubContext.Clients.All.SendAsync("DocumentDeleted", new { documentId = id });

        TempData["Success"] = "Đã xoá tài liệu thành công.";
        return RedirectToPage();
    }
}
