using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using ServiceLayer.Interfaces;
using ServiceLayer.Services;
using DataAccessLayer.Entities;

namespace PresentationLayer.Pages.Pricing;

[Authorize(Roles = "Student")]
public class CheckoutModel : PageModel
{
    private readonly IPackageService _packageService;
    private readonly IPaymentService _paymentService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IConfiguration _config;

    public CheckoutModel(
        IPackageService packageService,
        IPaymentService paymentService,
        UserManager<IdentityUser> userManager,
        IConfiguration config)
    {
        _packageService = packageService;
        _paymentService = paymentService;
        _userManager = userManager;
        _config = config;
    }

    public Package Package { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int packageId)
    {
        var package = await _packageService.GetPackageByIdAsync(packageId);
        if (package == null || !package.IsActive || package.Price <= 0)
        {
            return RedirectToPage("/Pricing/Index");
        }

        Package = package;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int packageId)
    {
        var package = await _packageService.GetPackageByIdAsync(packageId);
        if (package == null || !package.IsActive || package.Price <= 0)
        {
            return RedirectToPage("/Pricing/Index");
        }

        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Challenge();
        }

        var tmnCode = _config["Vnpay:TmnCode"];
        var hashSecret = _config["Vnpay:HashSecret"];
        var baseUrl = _config["Vnpay:BaseUrl"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
        var envReturnUrl = _config["Vnpay:ReturnUrl"];

        if (string.IsNullOrEmpty(tmnCode) || string.IsNullOrEmpty(hashSecret))
        {
            ModelState.AddModelError("", "Hệ thống chưa được cấu hình thông số thanh toán VNPAY trong file .env.");
            Package = package;
            return Page();
        }

        // Generate dynamic ReturnUrl
        string returnUrl = envReturnUrl ?? "";
        if (string.IsNullOrEmpty(returnUrl))
        {
            returnUrl = $"{Request.Scheme}://{Request.Host}/Pricing/VnpayReturn";
        }

        // Generate unique transaction reference
        string txnRef = DateTime.Now.Ticks.ToString();

        // 1. Create a Pending Transaction in database
        try
        {
            await _paymentService.CreatePendingTransactionAsync(userId, packageId, "VNPAY", txnRef);

            // 2. Build VNPAY Request URL
            var vnpay = new VnpayLibrary();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrEmpty(ipAddress) || ipAddress == "::1")
            {
                ipAddress = "127.0.0.1";
            }

            var now = DateTime.Now;
            var expire = now.AddMinutes(15);

            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", tmnCode);
            vnpay.AddRequestData("vnp_Amount", ((long)(package.Price * 100)).ToString());
            vnpay.AddRequestData("vnp_CreateDate", now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_ExpireDate", expire.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", ipAddress);
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "ThanhToanGoiCuocStandard");
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
            vnpay.AddRequestData("vnp_TxnRef", txnRef);

            var paymentUrl = vnpay.CreateRequestUrl(baseUrl, hashSecret);
            return Redirect(paymentUrl);
        }
        catch (Exception)
        {
            ModelState.AddModelError("", "Đã có lỗi xảy ra trong quá trình kết nối cổng thanh toán. Vui lòng thử lại.");
            Package = package;
            return Page();
        }
    }
}
