using DataAccessLayer.Context;
using DataAccessLayer.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PresentationLayer.ViewModels.Admin;

namespace PresentationLayer.Pages.Admin;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ChatbotDbContext _db;

    public IndexModel(UserManager<IdentityUser> userManager, ChatbotDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    public List<UserListItemViewModel> Users { get; set; } = new();

    public async Task OnGetAsync()
    {
        var allUsers = _userManager.Users.ToList();
        foreach (var user in allUsers)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "—";
            if (role == "Admin") continue;

            var assignedSubjects = await _db.TeacherSubjects
                .Where(ts => ts.TeacherId == user.Id)
                .Include(ts => ts.Subject)
                .Select(ts => ts.Subject.SubjectName)
                .ToListAsync();

            Users.Add(new UserListItemViewModel
            {
                UserId = user.Id,
                Email = user.Email ?? "",
                Role = role,
                AssignedSubjects = assignedSubjects
            });
        }
    }

    public async Task<IActionResult> OnPostDeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        var assignments = _db.TeacherSubjects.Where(ts => ts.TeacherId == userId);
        _db.TeacherSubjects.RemoveRange(assignments);
        await _db.SaveChangesAsync();

        await _userManager.DeleteAsync(user);
        TempData["Success"] = "Đã xoá tài khoản thành công.";
        return RedirectToPage();
    }
}
