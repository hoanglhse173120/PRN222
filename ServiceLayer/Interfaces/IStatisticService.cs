using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceLayer.Interfaces;

public class ChatStatDto
{
    public string Label { get; set; } = string.Empty;
    public int MessageCount { get; set; }
    public int SessionCount { get; set; }
    public int DocumentCount { get; set; }
    public int ActiveUserCount { get; set; }
}

public class UserSummaryDto
{
    public string? Id { get; set; }
    public string? Email { get; set; }
    public bool EmailConfirmed { get; set; }
}

public class DocumentSummaryDto
{
    public int DocumentId { get; set; }
    public string? FileName { get; set; }
    public string? SubjectName { get; set; }
    public double? FileSizeKb { get; set; }
    public DateTime? UploadedAt { get; set; }
}

public class ChatSessionSummaryDto
{
    public int SessionId { get; set; }
    public string? SessionName { get; set; }
    public string? SubjectName { get; set; }
    public string? UserEmail { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public interface IStatisticService
{
    Task<int> GetTotalUsersAsync();
    Task<int> GetTotalDocumentsAsync();
    Task<int> GetTotalChatSessionsAsync();
    Task<List<ChatStatDto>> GetChatStatsAsync(string filter);
    
    Task<List<UserSummaryDto>> GetRecentUsersAsync(int limit = 10);
    Task<List<DocumentSummaryDto>> GetRecentDocumentsAsync(int limit = 10);
    Task<List<ChatSessionSummaryDto>> GetRecentChatSessionsAsync(int limit = 10);
}
