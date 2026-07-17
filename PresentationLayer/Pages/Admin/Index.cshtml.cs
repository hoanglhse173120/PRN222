
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PresentationLayer.ViewModels.Admin;

using ServiceLayer.Interfaces;

namespace PresentationLayer.Pages.Admin;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ISubjectService _subjectService;
    private readonly IPaymentService _paymentService;

    public IndexModel(UserManager<IdentityUser> userManager, ISubjectService subjectService, IPaymentService paymentService)
    {
        _userManager = userManager;
        _subjectService = subjectService;
        _paymentService = paymentService;
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

            var assignedSubjects = await _subjectService.GetAssignedSubjectNamesAsync(user.Id);

            bool isPremium = false;
            string packageName = "—";
            DateTime? expiryDate = null;

            if (role == "Student")
            {
                var activeSub = await _paymentService.GetActiveSubscriptionAsync(user.Id);
                if (activeSub != null)
                {
                    isPremium = true;
                    packageName = activeSub.Package.PackageName;
                    expiryDate = activeSub.EndDate;
                }
            }

            Users.Add(new UserListItemViewModel
            {
                UserId = user.Id,
                Email = user.Email ?? "",
                Role = role,
                AssignedSubjects = assignedSubjects,
                IsPremium = isPremium,
                SubscriptionPackage = packageName,
                ExpiryDate = expiryDate
            });
        }
    }

    public async Task<IActionResult> OnPostDeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        await _subjectService.RemoveAllAssignmentsAsync(userId);

        await _userManager.DeleteAsync(user);
        TempData["Success"] = "Đã xoá tài khoản thành công.";
        return RedirectToPage();
    }
}
