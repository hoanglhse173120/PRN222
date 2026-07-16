using System.Linq;
using System.Threading.Tasks;
using DataAccessLayer.Context;
using DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace PresentationLayer.Seeders;

public static class PackageSeeder
{
    public static async Task SeedAsync(ChatbotDbContext context)
    {
        // Check if packages already exist
        if (await context.Packages.AnyAsync())
            return;

        var packages = new[]
        {
            new Package
            {
                PackageName = "Gói Học Tập (Standard)",
                Price = 50000,
                DurationInDays = 30,
                MaxQuestionsPerDay = 15,
                Description = "Tăng giới hạn hỏi đáp lên 15 câu hỏi/ngày. Phù hợp cho việc ôn tập và học tập hàng ngày.",
                IsActive = true
            },
            new Package
            {
                PackageName = "Gói Miễn Phí (Free)",
                Price = 0,
                DurationInDays = 9999,
                MaxQuestionsPerDay = 5,
                Description = "Gói mặc định của hệ thống với giới hạn 5 câu hỏi/ngày.",
                IsActive = true
            }
        };

        context.Packages.AddRange(packages);
        await context.SaveChangesAsync();
    }
}
