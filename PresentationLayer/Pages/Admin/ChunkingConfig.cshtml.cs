using PresentationLayer.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DataAccessLayer.Entities;
using DataAccessLayer.Repositories;

namespace PresentationLayer.Pages.Admin;

[Authorize(Roles = "Admin")]
public class ChunkingConfigModel : PageModel
{
    private readonly IRepository<ChunkingConfig> _configRepo;

    public ChunkingConfigModel(IRepository<ChunkingConfig> configRepo)
    {
        _configRepo = configRepo;
    }

    [BindProperty]
    public ChunkingConfig Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var configs = await _configRepo.GetAllAsync();
        var currentConfig = configs.FirstOrDefault();

        if (currentConfig == null)
        {
            // Fallback nếu chưa có db record
            Input = new ChunkingConfig { Strategy = "Words", MaxSize = 500, Overlap = 50 };
        }
        else
        {
            Input = currentConfig;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Input.Overlap >= Input.MaxSize)
        {
            ModelState.AddModelError("Input.Overlap", "Độ chồng chéo (Overlap) phải nhỏ hơn Kích thước tối đa (Max Size).");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var configs = await _configRepo.GetAllAsync();
        var currentConfig = configs.FirstOrDefault();

        if (currentConfig == null)
        {
            Input.UpdatedAt = DateTime.Now;
            await _configRepo.AddAsync(Input);
        }
        else
        {
            currentConfig.Strategy = Input.Strategy;
            currentConfig.MaxSize = Input.MaxSize;
            currentConfig.Overlap = Input.Overlap;
            currentConfig.UpdatedAt = DateTime.Now;
            _configRepo.Update(currentConfig);
        }

        await _configRepo.SaveChangesAsync();

        TempData["SuccessMessage"] = "Cập nhật cấu hình Chunking thành công!";
        return RedirectToPage();
    }
}
