using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceLayer.Interfaces;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace PresentationLayer.Pages.Admin;

[Authorize(Roles = "Admin")]
public class DashboardModel : PageModel
{
    private readonly IStatisticService _statisticService;

    public DashboardModel(IStatisticService statisticService)
    {
        _statisticService = statisticService;
    }

    public int TotalUsers { get; set; }
    public int TotalDocuments { get; set; }
    public int TotalChatSessions { get; set; }
    public int TotalTokensUsed { get; set; }
    public decimal EstimatedApiCost { get; set; }
    public double TotalDocumentSizeKb { get; set; }
    public Dictionary<string, int> UserRoleBreakdown { get; set; } = new();
    
    public string DailyChatStatsJson { get; set; } = "[]";

    public List<UserSummaryDto> RecentUsers { get; set; } = new();
    public List<DocumentSummaryDto> RecentDocuments { get; set; } = new();
    public List<ChatSessionSummaryDto> RecentChatSessions { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string Filter { get; set; } = "day";

    public async Task OnGetAsync()
    {
        TotalUsers = await _statisticService.GetTotalUsersAsync();
        TotalDocuments = await _statisticService.GetTotalDocumentsAsync();
        TotalChatSessions = await _statisticService.GetTotalChatSessionsAsync();
        TotalTokensUsed = await _statisticService.GetTotalTokensUsedAsync();
        EstimatedApiCost = (decimal)(TotalTokensUsed / 1000000.0 * 17500);
        TotalDocumentSizeKb = await _statisticService.GetTotalDocumentSizeKbAsync();
        UserRoleBreakdown = await _statisticService.GetUserRoleBreakdownAsync();
        
        var stats = await _statisticService.GetChatStatsAsync(Filter);
        DailyChatStatsJson = JsonSerializer.Serialize(stats);

        RecentUsers = await _statisticService.GetRecentUsersAsync(10);
        RecentDocuments = await _statisticService.GetRecentDocumentsAsync(10);
        RecentChatSessions = await _statisticService.GetRecentChatSessionsAsync(10);
    }
}
