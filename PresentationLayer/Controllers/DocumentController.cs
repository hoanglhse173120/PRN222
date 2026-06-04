using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PresentationLayer.ViewModels;
using ServiceLayer.Interfaces;
using ServiceLayer.DTOs;

namespace PresentationLayer.Controllers;

[Authorize]  // base: mọi user đã đăng nhập đều vào được
public class DocumentController : Controller
{
    private readonly IDocumentService _documentService;
    private readonly ISubjectService _subjectService;
    private readonly IWebHostEnvironment _env;

    // .ppt (binary cũ) bị loại vì OpenXml không đọc được, chỉ hỗ trợ .pptx
    private static readonly string[] AllowedExtensions = { ".pdf", ".docx", ".pptx", ".doc" };

    public DocumentController(IDocumentService documentService, ISubjectService subjectService, IWebHostEnvironment env)
    {
        _documentService = documentService;
        _subjectService = subjectService;
        _env = env;
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

    // GET: /Document/Upload  — chỉ Teacher
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Upload()
    {
        var vm = new DocumentUploadViewModel
        {
            Subjects = await _subjectService.GetAllAsync()
        };
        return View(vm);
    }

    // POST: /Document/Upload  — chỉ Teacher
    [Authorize(Roles = "Teacher")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(DocumentUploadViewModel vm)
    {
        vm.Subjects = await _subjectService.GetAllAsync();

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

    // POST: /Document/MarkIndexed/5  (extract text → chunk → lưu DB)  — chỉ Teacher
    [Authorize(Roles = "Teacher")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkIndexed(int id)
    {
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

    // POST: /Document/Delete/5  — chỉ Teacher
    [Authorize(Roles = "Teacher")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _documentService.DeleteAsync(id);
        TempData["Success"] = "Đã xóa tài liệu.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Document/Details/5
    // Kế thừa [Authorize(Roles = "Teacher,Student")] từ class
    public async Task<IActionResult> Details(int id)
    {
        var details = await _documentService.GetDetailsWithChunksAsync(id);
        if (details == null) return NotFound();
        return View(details);
    }
}
