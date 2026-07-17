using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PresentationLayer.ViewModels.Admin;
using Microsoft.AspNetCore.SignalR;
using PresentationLayer.Hubs;

namespace PresentationLayer.Pages.Admin;

[Authorize(Roles = "Admin")]
public class CreateModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IHubContext<ChatHub> _hubContext;

    public CreateModel(UserManager<IdentityUser> userManager, IHubContext<ChatHub> hubContext)
    {
        _userManager = userManager;
        _hubContext = hubContext;
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

        await _hubContext.Clients.All.SendAsync("NewUserRegistered", new { Email = Input.Email, Role = Input.Role });
        await _hubContext.Clients.All.SendAsync("UserListUpdated");

        TempData["Success"] = $"Đã tạo tài khoản {Input.Email} với vai trò {Input.Role}.";
        return RedirectToPage("Index");
    }
}
