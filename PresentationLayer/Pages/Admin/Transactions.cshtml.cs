using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceLayer.Interfaces;
using DataAccessLayer.Entities;

namespace PresentationLayer.Pages.Admin;

[Authorize(Roles = "Admin")]
public class TransactionsModel : PageModel
{
    private readonly IPaymentService _paymentService;

    public TransactionsModel(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public List<PaymentTransaction> Transactions { get; set; } = new();

    public async Task OnGetAsync()
    {
        Transactions = await _paymentService.GetAllTransactionsAsync();
    }
}
