using DataAccessLayer.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PresentationLayer.ViewModels.Admin;

namespace PresentationLayer.Pages.Admin;

[Authorize(Roles = "Admin")]
public class AssignSubjectsModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ChatbotDbContext _db;

    public AssignSubjectsModel(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ChatbotDbContext db)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
    }

    public AssignSubjectsViewModel Vm { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains("Teacher"))
        {
            TempData["Error"] = "Chỉ có thể phân công môn học cho Giảng viên.";
            return RedirectToPage("Index");
        }

        var allSubjects = await _db.Subjects
            .Select(s => new SubjectOption { SubjectId = s.SubjectId, SubjectName = s.SubjectName })
            .ToListAsync();

        var assignedIds = await _db.TeacherSubjects
            .Where(ts => ts.TeacherId == userId)
            .Select(ts => ts.SubjectId)
            .ToListAsync();

        var takenByOthers = await _db.TeacherSubjects
            .Where(ts => ts.TeacherId != userId)
            .Select(ts => ts.SubjectId)
            .ToListAsync();

        Vm = new AssignSubjectsViewModel
        {
            UserId = userId,
            UserEmail = user.Email ?? "",
            AllSubjects = allSubjects,
            AssignedSubjectIds = assignedIds,
            TakenByOtherIds = takenByOthers
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string userId, List<int> selectedSubjectIds)
    {
        selectedSubjectIds = selectedSubjectIds.Distinct().ToList();

        if (selectedSubjectIds.Count > 2)
        {
            TempData["Error"] = "Giảng viên chỉ được phân công tối đa 2 môn học.";
            return RedirectToPage(new { userId });
        }

        var conflicts = await _db.TeacherSubjects
            .Where(ts => ts.TeacherId != userId && selectedSubjectIds.Contains(ts.SubjectId))
            .Include(ts => ts.Subject)
            .ToListAsync();

        if (conflicts.Any())
        {
            var names = string.Join(", ", conflicts.Select(c => $"\"{c.Subject.SubjectName}\""));
            TempData["Error"] = $"Không thể phân công! Các môn {names} đã được phân cho giảng viên khác.";
            return RedirectToPage(new { userId });
        }

        var existing = _db.TeacherSubjects.Where(ts => ts.TeacherId == userId);
        _db.TeacherSubjects.RemoveRange(existing);

        foreach (var subjectId in selectedSubjectIds)
        {
            _db.TeacherSubjects.Add(new DataAccessLayer.Entities.TeacherSubject
            {
                TeacherId = userId,
                SubjectId = subjectId,
                AssignedAt = DateTime.Now
            });
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "Cập nhật phân công môn học thành công.";
        return RedirectToPage("Index");
    }
}
