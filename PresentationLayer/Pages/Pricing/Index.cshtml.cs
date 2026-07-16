using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceLayer.Interfaces;
using DataAccessLayer.Entities;

namespace PresentationLayer.Pages.Pricing;

[Authorize(Roles = "Student,Admin,Teacher")]
public class IndexModel : PageModel
{
    private readonly IPackageService _packageService;
    private readonly IPaymentService _paymentService;
    private readonly UserManager<IdentityUser> _userManager;

    public IndexModel(IPackageService packageService, IPaymentService paymentService, UserManager<IdentityUser> userManager)
    {
        _packageService = packageService;
        _paymentService = paymentService;
        _userManager = userManager;
    }

    public List<Package> Packages { get; set; } = new();
    public UserSubscription? ActiveSubscription { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Packages = await _packageService.GetAllActivePackagesAsync();
        
        var userId = _userManager.GetUserId(User);
        if (userId != null)
        {
            ActiveSubscription = await _paymentService.GetActiveSubscriptionAsync(userId);
        }

        return Page();
    }
}
