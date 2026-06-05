using DataAccessLayer.Context;
using DataAccessLayer.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PresentationLayer.ViewModels.Admin;

namespace PresentationLayer.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ChatbotDbContext _db;

    public AdminController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ChatbotDbContext db)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
    }

    // GET /Admin — Danh sách tất cả user (trừ admin)
    public async Task<IActionResult> Index()
    {
        var allUsers = _userManager.Users.ToList();
        var result = new List<UserListItemViewModel>();

        foreach (var user in allUsers)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "—";
            if (role == "Admin") continue; // ẩn admin khỏi danh sách

            var assignedSubjects = await _db.TeacherSubjects
                .Where(ts => ts.TeacherId == user.Id)
                .Include(ts => ts.Subject)
                .Select(ts => ts.Subject.SubjectName)
                .ToListAsync();

            result.Add(new UserListItemViewModel
            {
                UserId = user.Id,
                Email = user.Email ?? "",
                Role = role,
                AssignedSubjects = assignedSubjects
            });
        }

        return View(result);
    }

    // GET /Admin/Create
    public IActionResult Create() => View(new CreateUserViewModel());

    // POST /Admin/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = new IdentityUser
        {
            UserName = vm.Email,
            Email = vm.Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, vm.Password);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError("", e.Description);
            return View(vm);
        }

        await _userManager.AddToRoleAsync(user, vm.Role);
        TempData["Success"] = $"Đã tạo tài khoản {vm.Email} với vai trò {vm.Role}.";
        return RedirectToAction(nameof(Index));
    }

    // POST /Admin/DeleteUser
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        // Xoá phân công môn học
        var assignments = _db.TeacherSubjects.Where(ts => ts.TeacherId == userId);
        _db.TeacherSubjects.RemoveRange(assignments);
        await _db.SaveChangesAsync();

        await _userManager.DeleteAsync(user);
        TempData["Success"] = "Đã xoá tài khoản thành công.";
        return RedirectToAction(nameof(Index));
    }

    // GET /Admin/AssignSubjects/{userId}
    public async Task<IActionResult> AssignSubjects(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains("Teacher"))
        {
            TempData["Error"] = "Chỉ có thể phân công môn học cho Giảng viên.";
            return RedirectToAction(nameof(Index));
        }

        var allSubjects = await _db.Subjects
            .Select(s => new SubjectOption { SubjectId = s.SubjectId, SubjectName = s.SubjectName })
            .ToListAsync();

        var assignedIds = await _db.TeacherSubjects
            .Where(ts => ts.TeacherId == userId)
            .Select(ts => ts.SubjectId)
            .ToListAsync();

        // SubjectId nào đã được phân cho giảng viên KHÁC
        var takenByOthers = await _db.TeacherSubjects
            .Where(ts => ts.TeacherId != userId)
            .Select(ts => ts.SubjectId)
            .ToListAsync();

        var vm = new AssignSubjectsViewModel
        {
            UserId = userId,
            UserEmail = user.Email ?? "",
            AllSubjects = allSubjects,
            AssignedSubjectIds = assignedIds,
            TakenByOtherIds = takenByOthers
        };

        return View(vm);
    }

    // POST /Admin/AssignSubjects
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignSubjects(string userId, List<int> selectedSubjectIds)
    {
        selectedSubjectIds = selectedSubjectIds.Distinct().ToList();

        if (selectedSubjectIds.Count > 2)
        {
            TempData["Error"] = "Giảng viên chỉ được phân công tối đa 2 môn học.";
            return RedirectToAction(nameof(AssignSubjects), new { userId });
        }

        // Kiểm tra xem có môn nào đang được phân cho giảng viên khác không
        var conflicts = await _db.TeacherSubjects
            .Where(ts => ts.TeacherId != userId && selectedSubjectIds.Contains(ts.SubjectId))
            .Include(ts => ts.Subject)
            .ToListAsync();

        if (conflicts.Any())
        {
            var names = string.Join(", ", conflicts.Select(c => $"\"{c.Subject.SubjectName}\""));
            TempData["Error"] = $"Không thể phân công! Các môn {names} đã được phân cho giảng viên khác.";
            return RedirectToAction(nameof(AssignSubjects), new { userId });
        }

        // Xoá tất cả phân công cũ của giảng viên này
        var existing = _db.TeacherSubjects.Where(ts => ts.TeacherId == userId);
        _db.TeacherSubjects.RemoveRange(existing);

        // Thêm phân công mới
        foreach (var subjectId in selectedSubjectIds)
        {
            _db.TeacherSubjects.Add(new TeacherSubject
            {
                TeacherId = userId,
                SubjectId = subjectId,
                AssignedAt = DateTime.Now
            });
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "Cập nhật phân công môn học thành công.";
        return RedirectToAction(nameof(Index));
    }
}
