using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLayer.Context;
using DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using ServiceLayer.Interfaces;

namespace ServiceLayer.Services;

public class StatisticService : IStatisticService
{
    private readonly ChatbotDbContext _context;

    public StatisticService(ChatbotDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetTotalUsersAsync()
    {
        return await _context.Users.CountAsync();
    }

    public async Task<int> GetTotalDocumentsAsync()
    {
        return await _context.Documents.CountAsync();
    }

    public async Task<int> GetTotalChatSessionsAsync()
    {
        return await _context.ChatSessions.CountAsync();
    }

    public async Task<int> GetTotalTokensUsedAsync()
    {
        return await _context.ChatMessages.SumAsync(m => m.TokenCount);
    }

    public async Task<List<ChatStatDto>> GetChatStatsAsync(string filter)
    {
        var result = new List<ChatStatDto>();
        var now = DateTime.Today;

        if (filter == "week") // 4 weeks
        {
            var startDate = now.AddDays(-27); // 4 weeks = 28 days
            var statsFromDb = await _context.ChatMessages.Where(m => m.Timestamp >= startDate).ToListAsync();
            var sessionsFromDb = await _context.ChatSessions.Where(s => s.CreatedAt >= startDate).ToListAsync();
            var docsFromDb = await _context.Documents.Where(d => d.UploadedAt >= startDate).ToListAsync();

            for (int i = 3; i >= 0; i--)
            {
                var startOfWeek = now.AddDays(-(i * 7 + 6));
                var endOfWeek = now.AddDays(-(i * 7));
                var endBoundary = endOfWeek.AddDays(1);
                
                int msgCount = statsFromDb.Count(m => m.Timestamp >= startOfWeek && m.Timestamp < endBoundary);
                int tokenCount = statsFromDb.Where(m => m.Timestamp >= startOfWeek && m.Timestamp < endBoundary).Sum(m => m.TokenCount);
                int sessionCount = sessionsFromDb.Count(s => s.CreatedAt >= startOfWeek && s.CreatedAt < endBoundary);
                int docCount = docsFromDb.Count(d => d.UploadedAt >= startOfWeek && d.UploadedAt < endBoundary);
                int activeUserCount = sessionsFromDb.Where(s => s.CreatedAt >= startOfWeek && s.CreatedAt < endBoundary).Select(s => s.UserId).Distinct().Count();

                result.Add(new ChatStatDto { Label = $"{startOfWeek:dd/MM}-{endOfWeek:dd/MM}", MessageCount = msgCount, SessionCount = sessionCount, DocumentCount = docCount, ActiveUserCount = activeUserCount, TokenCount = tokenCount });
            }
        }
        else if (filter == "month") // 12 months
        {
            var startDate = now.AddMonths(-11);
            startDate = new DateTime(startDate.Year, startDate.Month, 1);
            var statsFromDb = await _context.ChatMessages.Where(m => m.Timestamp >= startDate).ToListAsync();
            var sessionsFromDb = await _context.ChatSessions.Where(s => s.CreatedAt >= startDate).ToListAsync();
            var docsFromDb = await _context.Documents.Where(d => d.UploadedAt >= startDate).ToListAsync();

            for (int i = 11; i >= 0; i--)
            {
                var targetMonth = now.AddMonths(-i);
                
                int msgCount = statsFromDb.Count(m => m.Timestamp.HasValue && m.Timestamp.Value.Month == targetMonth.Month && m.Timestamp.Value.Year == targetMonth.Year);
                int tokenCount = statsFromDb.Where(m => m.Timestamp.HasValue && m.Timestamp.Value.Month == targetMonth.Month && m.Timestamp.Value.Year == targetMonth.Year).Sum(m => m.TokenCount);
                int sessionCount = sessionsFromDb.Count(s => s.CreatedAt.HasValue && s.CreatedAt.Value.Month == targetMonth.Month && s.CreatedAt.Value.Year == targetMonth.Year);
                int docCount = docsFromDb.Count(d => d.UploadedAt.HasValue && d.UploadedAt.Value.Month == targetMonth.Month && d.UploadedAt.Value.Year == targetMonth.Year);
                int activeUserCount = sessionsFromDb.Where(s => s.CreatedAt.HasValue && s.CreatedAt.Value.Month == targetMonth.Month && s.CreatedAt.Value.Year == targetMonth.Year).Select(s => s.UserId).Distinct().Count();

                result.Add(new ChatStatDto { Label = targetMonth.ToString("MM/yyyy"), MessageCount = msgCount, SessionCount = sessionCount, DocumentCount = docCount, ActiveUserCount = activeUserCount, TokenCount = tokenCount });
            }
        }
        else if (filter == "year") // 5 years
        {
            var startDate = new DateTime(now.Year - 4, 1, 1);
            var statsFromDb = await _context.ChatMessages.Where(m => m.Timestamp >= startDate).ToListAsync();
            var sessionsFromDb = await _context.ChatSessions.Where(s => s.CreatedAt >= startDate).ToListAsync();
            var docsFromDb = await _context.Documents.Where(d => d.UploadedAt >= startDate).ToListAsync();

            for (int i = 4; i >= 0; i--)
            {
                int targetYear = now.Year - i;
                
                int msgCount = statsFromDb.Count(m => m.Timestamp.HasValue && m.Timestamp.Value.Year == targetYear);
                int tokenCount = statsFromDb.Where(m => m.Timestamp.HasValue && m.Timestamp.Value.Year == targetYear).Sum(m => m.TokenCount);
                int sessionCount = sessionsFromDb.Count(s => s.CreatedAt.HasValue && s.CreatedAt.Value.Year == targetYear);
                int docCount = docsFromDb.Count(d => d.UploadedAt.HasValue && d.UploadedAt.Value.Year == targetYear);
                int activeUserCount = sessionsFromDb.Where(s => s.CreatedAt.HasValue && s.CreatedAt.Value.Year == targetYear).Select(s => s.UserId).Distinct().Count();

                result.Add(new ChatStatDto { Label = targetYear.ToString(), MessageCount = msgCount, SessionCount = sessionCount, DocumentCount = docCount, ActiveUserCount = activeUserCount, TokenCount = tokenCount });
            }
        }
        else // default to "day" (7 days)
        {
            var startDate = now.AddDays(-6);
            var statsFromDb = await _context.ChatMessages.Where(m => m.Timestamp >= startDate).ToListAsync();
            var sessionsFromDb = await _context.ChatSessions.Where(s => s.CreatedAt >= startDate).ToListAsync();
            var docsFromDb = await _context.Documents.Where(d => d.UploadedAt >= startDate).ToListAsync();

            for (int i = 6; i >= 0; i--)
            {
                var targetDay = now.AddDays(-i);
                
                int msgCount = statsFromDb.Count(m => m.Timestamp.HasValue && m.Timestamp.Value.Date == targetDay);
                int tokenCount = statsFromDb.Where(m => m.Timestamp.HasValue && m.Timestamp.Value.Date == targetDay).Sum(m => m.TokenCount);
                int sessionCount = sessionsFromDb.Count(s => s.CreatedAt.HasValue && s.CreatedAt.Value.Date == targetDay);
                int docCount = docsFromDb.Count(d => d.UploadedAt.HasValue && d.UploadedAt.Value.Date == targetDay);
                int activeUserCount = sessionsFromDb.Where(s => s.CreatedAt.HasValue && s.CreatedAt.Value.Date == targetDay).Select(s => s.UserId).Distinct().Count();

                result.Add(new ChatStatDto { Label = targetDay.ToString("dd/MM"), MessageCount = msgCount, SessionCount = sessionCount, DocumentCount = docCount, ActiveUserCount = activeUserCount, TokenCount = tokenCount });
            }
        }

        return result;
    }

    public async Task<List<RevenueStatDto>> GetRevenueStatsAsync(string filter)
    {
        var result = new List<RevenueStatDto>();
        var now = DateTime.Today;

        var allTransactions = await _context.PaymentTransactions
            .Where(pt => pt.Status == "Success")
            .ToListAsync();

        if (filter == "year")
        {
            var startDate = new DateTime(now.Year - 4, 1, 1);
            
            for (int i = 4; i >= 0; i--)
            {
                int targetYear = now.Year - i;
                
                var txnsInYear = allTransactions.Where(t => t.TransactionDate.Year == targetYear);
                
                decimal revenue = txnsInYear.Sum(t => t.Amount);
                int subCount = txnsInYear.Count();

                result.Add(new RevenueStatDto { Label = targetYear.ToString(), Revenue = revenue, SubscriptionCount = subCount });
            }
        }
        else if (filter == "month")
        {
            for (int i = 11; i >= 0; i--)
            {
                var targetMonth = now.AddMonths(-i);
                
                var txnsInMonth = allTransactions.Where(t => t.TransactionDate.Month == targetMonth.Month && t.TransactionDate.Year == targetMonth.Year);
                
                decimal revenue = txnsInMonth.Sum(t => t.Amount);
                int subCount = txnsInMonth.Count();

                result.Add(new RevenueStatDto { Label = targetMonth.ToString("MM/yyyy"), Revenue = revenue, SubscriptionCount = subCount });
            }
        }
        else if (filter == "week")
        {
            for (int i = 3; i >= 0; i--)
            {
                var startOfWeek = now.AddDays(-(i * 7 + 6));
                var endOfWeek = now.AddDays(-(i * 7));
                var endBoundary = endOfWeek.AddDays(1);
                
                var txnsInWeek = allTransactions.Where(t => t.TransactionDate >= startOfWeek && t.TransactionDate < endBoundary);
                
                decimal revenue = txnsInWeek.Sum(t => t.Amount);
                int subCount = txnsInWeek.Count();

                result.Add(new RevenueStatDto { Label = $"{startOfWeek:dd/MM}-{endOfWeek:dd/MM}", Revenue = revenue, SubscriptionCount = subCount });
            }
        }
        else // default to "day"
        {
            for (int i = 6; i >= 0; i--)
            {
                var targetDay = now.AddDays(-i);
                
                var txnsInDay = allTransactions.Where(t => t.TransactionDate.Date == targetDay);
                
                decimal revenue = txnsInDay.Sum(t => t.Amount);
                int subCount = txnsInDay.Count();

                result.Add(new RevenueStatDto { Label = targetDay.ToString("dd/MM"), Revenue = revenue, SubscriptionCount = subCount });
            }
        }

        return result;
    }

    public async Task<decimal> GetTotalRevenueAsync()
    {
        return await _context.PaymentTransactions
            .Where(pt => pt.Status == "Success")
            .SumAsync(pt => pt.Amount);
    }

    public async Task<int> GetActiveSubscriptionsAsync()
    {
        var now = DateTime.Now;
        return await _context.UserSubscriptions
            .Where(us => us.IsActive && us.StartDate <= now && us.EndDate >= now)
            .CountAsync();
    }

    public async Task<double> GetTotalDocumentSizeKbAsync()
    {
        var size = await _context.Documents.SumAsync(d => d.FileSizeKb);
        return size.HasValue ? (double)size.Value : 0;
    }

    public async Task<Dictionary<string, int>> GetUserRoleBreakdownAsync()
    {
        var roleBreakdown = new Dictionary<string, int>();
        
        var roles = await _context.Roles.ToListAsync();
        var userRoles = await _context.UserRoles.ToListAsync();
        
        foreach (var role in roles)
        {
            var count = userRoles.Count(ur => ur.RoleId == role.Id);
            if (count > 0)
            {
                roleBreakdown[role.Name ?? "Unknown"] = count;
            }
        }
        
        // Count users with no roles (optional, just in case)
        var usersWithRoles = userRoles.Select(ur => ur.UserId).Distinct().ToList();
        var totalUsers = await _context.Users.CountAsync();
        var usersWithoutRoles = totalUsers - usersWithRoles.Count;
        
        if (usersWithoutRoles > 0)
        {
            roleBreakdown["No Role"] = usersWithoutRoles;
        }

        return roleBreakdown;
    }

    public async Task<List<UserSummaryDto>> GetRecentUsersAsync(int limit = 10)
    {
        return await _context.Users
            .OrderByDescending(u => u.Id) // In ASP.NET Identity, Id is usually string. Better to order by another field if possible, but Id is fine for now, or just take random. Wait, let's just take the first ones if no CreatedAt exists.
            .Take(limit)
            .Select(u => new UserSummaryDto
            {
                Id = u.Id,
                Email = u.Email,
                EmailConfirmed = u.EmailConfirmed
            })
            .ToListAsync();
    }

    public async Task<List<DocumentSummaryDto>> GetRecentDocumentsAsync(int limit = 10)
    {
        return await _context.Documents
            .Include(d => d.Subject)
            .OrderByDescending(d => d.UploadedAt)
            .Take(limit)
            .Select(d => new DocumentSummaryDto
            {
                DocumentId = d.DocumentId,
                FileName = d.FileName,
                SubjectName = d.Subject != null ? d.Subject.SubjectName : "Không có",
                FileSizeKb = (double?)d.FileSizeKb,
                UploadedAt = d.UploadedAt
            })
            .ToListAsync();
    }

    public async Task<List<ChatSessionSummaryDto>> GetRecentChatSessionsAsync(int limit = 10)
    {
        return await _context.ChatSessions
            .Include(s => s.Subject)
            .Include(s => s.User)
            .OrderByDescending(s => s.CreatedAt)
            .Take(limit)
            .Select(s => new ChatSessionSummaryDto
            {
                SessionId = s.SessionId,
                SessionName = s.SessionName,
                SubjectName = s.Subject != null ? s.Subject.SubjectName : "Không có",
                UserEmail = s.User != null ? s.User.Email : "Ẩn danh",
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();
    }
}
