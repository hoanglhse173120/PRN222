using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceLayer.Interfaces;
using System.Threading.Tasks;

using System.Text;

namespace PresentationLayer.Pages.Admin;

[Authorize(Roles = "Admin")]
public class RevenueModel : PageModel
{
    private readonly IStatisticService _statisticService;
    private readonly IPaymentService _paymentService;

    public RevenueModel(IStatisticService statisticService, IPaymentService paymentService)
    {
        _statisticService = statisticService;
        _paymentService = paymentService;
    }

    public decimal TotalRevenue { get; set; }
    public int ActiveSubscriptions { get; set; }

    public async Task OnGetAsync()
    {
        TotalRevenue = await _statisticService.GetTotalRevenueAsync();
        ActiveSubscriptions = await _statisticService.GetActiveSubscriptionsAsync();
    }

    public async Task<IActionResult> OnGetExportCsvAsync()
    {
        var allTransactions = await _paymentService.GetAllTransactionsAsync();
        var transactions = allTransactions.Where(t => t.Status == "Success").ToList();

        var builder = new StringBuilder();
        builder.AppendLine("TransactionId,UserEmail,PackageName,Amount,Date,Status,TransactionRef");
        
        foreach (var txn in transactions)
        {
            var email = txn.User?.Email ?? "Unknown";
            var package = txn.Package?.PackageName ?? "Unknown";
            builder.AppendLine($"{txn.TransactionId},{email},{package},{txn.Amount},{txn.TransactionDate:yyyy-MM-dd HH:mm},{txn.Status},{txn.TransactionReference}");
        }

        var bytes = Encoding.UTF8.GetBytes(builder.ToString());
        return File(bytes, "text/csv", $"DoanhThu_{System.DateTime.Now:yyyyMMdd_HHmm}.csv");
    }
}
