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
using Microsoft.AspNetCore.SignalR;
using PresentationLayer.Hubs;

namespace PresentationLayer.Pages.Pricing;

[Authorize]
public class VnpayReturnModel : PageModel
{
    private readonly IPaymentService _paymentService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IConfiguration _config;
    private readonly IHubContext<ChatHub> _hubContext;

    public VnpayReturnModel(
        IPaymentService paymentService,
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IConfiguration config,
        IHubContext<ChatHub> hubContext)
    {
        _paymentService = paymentService;
        _userManager = userManager;
        _signInManager = signInManager;
        _config = config;
        _hubContext = hubContext;
    }

    public string Message { get; set; } = string.Empty;
    public bool IsSuccess { get; set; } = false;
    public decimal Amount { get; set; }
    public string TxnRef { get; set; } = string.Empty;
    public string TransactionNo { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync()
    {
        var hashSecret = _config["Vnpay:HashSecret"];
        if (string.IsNullOrEmpty(hashSecret))
        {
            Message = "Cấu hình VNPAY_HashSecret thiếu trong .env.";
            return Page();
        }

        var vnpay = new VnpayLibrary();
        foreach (var key in Request.Query.Keys)
        {
            var value = Request.Query[key].ToString();
            if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
            {
                vnpay.AddResponseData(key, value);
            }
        }

        string vnp_TxnRef = vnpay.GetResponseData("vnp_TxnRef");
        string vnp_SecureHash = Request.Query["vnp_SecureHash"].ToString();
        string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
        string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
        string vnp_AmountStr = vnpay.GetResponseData("vnp_Amount");
        string vnp_TransactionNo = vnpay.GetResponseData("vnp_TransactionNo");

        TxnRef = vnp_TxnRef;
        TransactionNo = vnp_TransactionNo;

        if (long.TryParse(vnp_AmountStr, out var rawAmount))
        {
            Amount = rawAmount / 100m;
        }

        // Validate signature
        bool isValidSignature = vnpay.ValidateSignature(vnp_SecureHash, hashSecret);

        if (isValidSignature)
        {
            if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
            {
                // Payment was successful
                var updated = await _paymentService.CompleteTransactionAsync(vnp_TxnRef, isSuccess: true);
                if (updated)
                {
                    IsSuccess = true;
                    Message = "Giao dịch thanh toán nâng cấp tài khoản thành công!";

                    // Emit event to Admin Dashboard/Revenue page
                    await _hubContext.Clients.All.SendAsync("NewTransactionCompleted", new { Amount = Amount });

                    // Refresh user sign-in to reload claims immediately
                    var user = await _userManager.GetUserAsync(User);
                    if (user != null)
                    {
                        await _signInManager.RefreshSignInAsync(user);
                    }
                }
                else
                {
                    Message = "Không thể cập nhật trạng thái giao dịch hoặc giao dịch đã được xử lý trước đó.";
                }
            }
            else
            {
                // Payment failed on VNPAY
                await _paymentService.CompleteTransactionAsync(vnp_TxnRef, isSuccess: false);
                Message = $"Giao dịch không thành công. Mã phản hồi VNPAY: {vnp_ResponseCode}";
            }
        }
        else
        {
            Message = "Chữ ký số không hợp lệ. Giao dịch có thể đã bị can thiệp.";
        }

        return Page();
    }
}
