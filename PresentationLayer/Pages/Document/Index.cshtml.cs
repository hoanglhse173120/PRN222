using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PresentationLayer.ViewModels;
using ServiceLayer.Interfaces;
using DataAccessLayer.Context;

namespace PresentationLayer.Pages.Document;

[Authorize(Roles = "Teacher,Student,Admin")]
public class IndexModel : PageModel
{
    private readonly IDocumentService _documentService;
    private readonly ISubjectService _subjectService;
    private readonly IWebHostEnvironment _env;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ChatbotDbContext _db;

    public IndexModel(
        IDocumentService documentService, 
        ISubjectService subjectService,
        IWebHostEnvironment env,
        UserManager<IdentityUser> userManager,
        ChatbotDbContext db)
    {
        _documentService = documentService;
        _subjectService = subjectService;
        _env = env;
        _userManager = userManager;
        _db = db;
    }

    [BindProperty(SupportsGet = true)]
    public int? SubjectId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string Filter { get; set; } = "all";

    public DocumentIndexViewModel Vm { get; set; } = null!;

    public async Task OnGetAsync()
    {
        var subjects = await _subjectService.GetAllAsync();
        var documents = SubjectId.HasValue
            ? await _documentService.GetBySubjectAsync(SubjectId.Value)
            : await _documentService.GetAllAsync();

        documents = Filter switch
        {
            "indexed" => documents.Where(d => d.IsIndexed == true),
            "pending" => documents.Where(d => d.IsIndexed != true),
            _ => documents
        };

        Vm = new DocumentIndexViewModel
        {
            Documents = documents,
            Subjects = subjects,
            SelectedSubjectId = SubjectId,
            Filter = Filter
        };
    }

    public async Task<IActionResult> OnPostMarkIndexedAsync(int id)
    {
        if (User.IsInRole("Teacher"))
        {
            var doc = await _documentService.GetAllAsync();
            var target = doc.FirstOrDefault(d => d.DocumentID == id);
            if (target == null) return NotFound();

            var userId = _userManager.GetUserId(User) ?? "";
            var assignedIds = await _db.TeacherSubjects
                .Where(ts => ts.TeacherId == userId)
                .Select(ts => ts.SubjectId)
                .ToListAsync();

            if (!assignedIds.Contains(target.SubjectID))
            {
                TempData["Error"] = "Bạn chỉ được index tài liệu thuộc môn học được phân công.";
                return RedirectToPage();
            }
        }

        try
        {
            var chunkCount = await _documentService.IndexDocumentAsync(id, _env.WebRootPath);
            TempData["Success"] = $"Index thành công! Đã tách thành {chunkCount} chunks và lưu vào database.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Lỗi khi index: {ex.Message}";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        if (User.IsInRole("Teacher"))
        {
            var doc = await _documentService.GetAllAsync();
            var target = doc.FirstOrDefault(d => d.DocumentID == id);
            if (target == null) return NotFound();

            var userId = _userManager.GetUserId(User) ?? "";
            var assignedIds = await _db.TeacherSubjects
                .Where(ts => ts.TeacherId == userId)
                .Select(ts => ts.SubjectId)
                .ToListAsync();

            if (!assignedIds.Contains(target.SubjectID))
            {
                TempData["Error"] = "Bạn chỉ được xóa tài liệu thuộc môn học được phân công.";
                return RedirectToPage();
            }
        }

        await _documentService.DeleteAsync(id);
        TempData["Success"] = "Đã xoá tài liệu thành công.";
        return RedirectToPage();
    }
}
