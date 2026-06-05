using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PresentationLayer.ViewModels;
using ServiceLayer.Interfaces;
using ServiceLayer.DTOs;
using DataAccessLayer.Context;

namespace PresentationLayer.Controllers;

[Authorize]  // base: mọi user đã đăng nhập đều vào được
public class DocumentController : Controller
{
    private readonly IDocumentService _documentService;
    private readonly ISubjectService _subjectService;
    private readonly IWebHostEnvironment _env;
    private readonly ChatbotDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    // .ppt (binary cũ) bị loại vì OpenXml không đọc được, chỉ hỗ trợ .pptx
    private static readonly string[] AllowedExtensions = { ".pdf", ".docx", ".pptx", ".doc" };

    public DocumentController(
        IDocumentService documentService,
        ISubjectService subjectService,
        IWebHostEnvironment env,
        ChatbotDbContext db,
        UserManager<IdentityUser> userManager)
    {
        _documentService = documentService;
        _subjectService = subjectService;
        _env = env;
        _db = db;
        _userManager = userManager;
    }

    // GET: /Document  (danh sách tài liệu, lọc theo môn & trạng thái)
    // Kế thừa [Authorize(Roles = "Teacher,Student")] từ class
    public async Task<IActionResult> Index(int? subjectId, string filter = "all")
    {
        var subjects = await _subjectService.GetAllAsync();
        var documents = subjectId.HasValue
            ? await _documentService.GetBySubjectAsync(subjectId.Value)
            : await _documentService.GetAllAsync();

        documents = filter switch
        {
            "indexed" => documents.Where(d => d.IsIndexed),
            "pending" => documents.Where(d => !d.IsIndexed),
            _ => documents
        };

        var vm = new DocumentIndexViewModel
        {
            Documents = documents,
            Subjects = subjects,
            SelectedSubjectId = subjectId,
            Filter = filter
        };
        return View(vm);
    }

    // GET: /Document/Upload  — Teacher chỉ thấy môn được phân công
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<IActionResult> Upload()
    {
        var allSubjects = await _subjectService.GetAllAsync();
        IEnumerable<SubjectDto> filteredSubjects = allSubjects;

        if (User.IsInRole("Teacher"))
        {
            var userId = _userManager.GetUserId(User) ?? "";
            var assignedIds = await _db.TeacherSubjects
                .Where(ts => ts.TeacherId == userId)
                .Select(ts => ts.SubjectId)
                .ToListAsync();

            filteredSubjects = allSubjects.Where(s => assignedIds.Contains(s.SubjectID));
        }

        var vm = new DocumentUploadViewModel { Subjects = filteredSubjects };
        return View(vm);
    }

    // POST: /Document/Upload  — Teacher chỉ được upload vào môn được phân công
    [Authorize(Roles = "Teacher,Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(DocumentUploadViewModel vm)
    {
        // Nạp lại danh sách môn phù hợp cho form
        var allSubjects = await _subjectService.GetAllAsync();
        if (User.IsInRole("Teacher"))
        {
            var userId = _userManager.GetUserId(User) ?? "";
            var assignedIds = await _db.TeacherSubjects
                .Where(ts => ts.TeacherId == userId)
                .Select(ts => ts.SubjectId)
                .ToListAsync();

            // Validate: Teacher chỉ được upload vào môn được phân công
            if (!assignedIds.Contains(vm.SubjectId))
            {
                ModelState.AddModelError("", "Bạn không có quyền upload tài liệu cho môn học này.");
                vm.Subjects = allSubjects.Where(s => assignedIds.Contains(s.SubjectID));
                return View(vm);
            }

            vm.Subjects = allSubjects.Where(s => assignedIds.Contains(s.SubjectID));
        }
        else
        {
            vm.Subjects = allSubjects;
        }

        if (!ModelState.IsValid) return View(vm);

        var file = vm.File;
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!AllowedExtensions.Contains(ext))
        {
            ModelState.AddModelError("File", "Chỉ chấp nhận file PDF, DOCX, PPTX.");
            return View(vm);
        }

        // Lưu file vào wwwroot/uploads/<subjectId>/
        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", vm.SubjectId.ToString());
        Directory.CreateDirectory(uploadDir);

        var uniqueName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
        var fullPath = Path.Combine(uploadDir, uniqueName);

        await using (var stream = new FileStream(fullPath, FileMode.Create))
            await file.CopyToAsync(stream);

        var relativePath = $"/uploads/{vm.SubjectId}/{uniqueName}";
        var fileSizeKB = file.Length / 1024;

        await _documentService.UploadAsync(
            vm.SubjectId,
            file.FileName,
            ext.TrimStart('.').ToUpper(),
            relativePath,
            fileSizeKB
        );

        TempData["Success"] = $"Upload \"{file.FileName}\" thành công! Tài liệu đang chờ được index.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Document/MarkIndexed/5  — Teacher chỉ được index tài liệu môn được phân công
    [Authorize(Roles = "Teacher,Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkIndexed(int id)
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
                return RedirectToAction(nameof(Index));
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
        return RedirectToAction(nameof(Index));
    }

    // POST: /Document/Delete/5  — Teacher chỉ được xóa tài liệu môn được phân công
    [Authorize(Roles = "Teacher,Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
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
                return RedirectToAction(nameof(Index));
            }
        }

        await _documentService.DeleteAsync(id);
        TempData["Success"] = "Đã xóa tài liệu.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Document/Details/5  — chỉ Admin và Teacher mới xem chi tiết chunks
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Details(int id)
    {
        var details = await _documentService.GetDetailsWithChunksAsync(id);
        if (details == null) return NotFound();
        return View(details);
    }
}
