
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PresentationLayer.ViewModels.Admin;

using ServiceLayer.Interfaces;

namespace PresentationLayer.Pages.Admin;

[Authorize(Roles = "Admin")]
public class AssignSubjectsModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ISubjectService _subjectService;

    public AssignSubjectsModel(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ISubjectService subjectService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _subjectService = subjectService;
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

        var subjects = await _subjectService.GetAllAsync();
        var allSubjects = subjects.Select(s => new SubjectOption { SubjectId = s.SubjectID, SubjectName = s.SubjectName }).ToList();

        var assignedIds = await _subjectService.GetAssignedSubjectIdsAsync(userId);
        var takenByOthers = await _subjectService.GetTakenByOthersSubjectIdsAsync(userId);

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
        var result = await _subjectService.AssignSubjectsToTeacherAsync(userId, selectedSubjectIds);

        if (!result.Success)
        {
            TempData["Error"] = result.ErrorMessage;
            return RedirectToPage(new { userId });
        }

        TempData["Success"] = "Cập nhật phân công môn học thành công.";
        return RedirectToPage("Index");
    }
}
