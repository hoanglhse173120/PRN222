using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PresentationLayer.ViewModels.Admin;

namespace PresentationLayer.Pages.Admin;

[Authorize(Roles = "Admin")]
public class CreateModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;

    public CreateModel(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    [BindProperty]
    public CreateUserViewModel Input { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var user = new IdentityUser
        {
            UserName = Input.Email,
            Email = Input.Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, Input.Password);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError("", e.Description);
            return Page();
        }

        await _userManager.AddToRoleAsync(user, Input.Role);
        TempData["Success"] = $"Đã tạo tài khoản {Input.Email} với vai trò {Input.Role}.";
        return RedirectToPage("Index");
    }
}
