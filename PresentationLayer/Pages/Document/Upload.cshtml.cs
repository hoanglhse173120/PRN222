using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PresentationLayer.ViewModels;
using ServiceLayer.Interfaces;
using ServiceLayer.DTOs;

namespace PresentationLayer.Pages.Document;

[Authorize(Roles = "Teacher")]
public class UploadModel : PageModel
{
    private readonly IDocumentService _documentService;
    private readonly ISubjectService _subjectService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IWebHostEnvironment _env;

    private static readonly string[] AllowedExtensions = { ".pdf", ".docx", ".pptx", ".doc" };

    public UploadModel(
        IDocumentService documentService, 
        ISubjectService subjectService, 
        UserManager<IdentityUser> userManager,
        IWebHostEnvironment env)
    {
        _documentService = documentService;
        _subjectService = subjectService;
        _userManager = userManager;
        _env = env;
    }

    [BindProperty]
    public DocumentUploadViewModel Input { get; set; } = new();

    public async Task OnGetAsync()
    {
        var allSubjects = await _subjectService.GetAllAsync();
        
        var userId = _userManager.GetUserId(User) ?? "";
        var assignedIds = await _subjectService.GetAssignedSubjectIdsAsync(userId);

        Input.Subjects = allSubjects.Where(s => assignedIds.Contains(s.SubjectID));
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var allSubjects = await _subjectService.GetAllAsync();
        
        var userId = _userManager.GetUserId(User) ?? "";
        var assignedIds = await _subjectService.GetAssignedSubjectIdsAsync(userId);

        if (!assignedIds.Contains(Input.SubjectId))
        {
            ModelState.AddModelError("", "Bạn không có quyền upload tài liệu cho môn học này.");
            Input.Subjects = allSubjects.Where(s => assignedIds.Contains(s.SubjectID));
            return Page();
        }

        Input.Subjects = allSubjects.Where(s => assignedIds.Contains(s.SubjectID));

        if (Input.File == null || Input.File.Length == 0)
        {
            ModelState.AddModelError("Input.File", "Vui lòng chọn file.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var file = Input.File!;
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!AllowedExtensions.Contains(ext))
        {
            ModelState.AddModelError("Input.File", "Chỉ chấp nhận file PDF, DOCX, PPTX.");
            return Page();
        }

        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", Input.SubjectId.ToString());
        Directory.CreateDirectory(uploadDir);

        var uniqueName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
        var fullPath = Path.Combine(uploadDir, uniqueName);

        await using (var stream = new FileStream(fullPath, FileMode.Create))
            await file.CopyToAsync(stream);

        var relativePath = $"/uploads/{Input.SubjectId}/{uniqueName}";
        var fileSizeKB = file.Length / 1024;
        

        var newDoc = await _documentService.UploadAsync(
            Input.SubjectId,
            file.FileName,
            ext.TrimStart('.').ToUpper(),
            relativePath,
            fileSizeKB,
            userId
        );

        var chunkCount = await _documentService.IndexDocumentAsync(newDoc.DocumentID, _env.WebRootPath);

        TempData["Success"] = $"Upload \"{file.FileName}\" thành công! Đã tự động phân tách thành {chunkCount} đoạn.";
        return RedirectToPage("Index");
    }
}
