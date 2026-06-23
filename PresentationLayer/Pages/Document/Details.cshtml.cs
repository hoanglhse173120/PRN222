using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceLayer.DTOs;
using ServiceLayer.Interfaces;

namespace PresentationLayer.Pages.Document;

[Authorize(Roles = "Teacher,Student,Admin")]
public class DetailsModel : PageModel
{
    private readonly IDocumentService _documentService;
    
    public DetailsModel(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    public DocumentDetailsDto DocDetails { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var details = await _documentService.GetDetailsWithChunksAsync(id);
        if (details == null) return NotFound();
        DocDetails = details;
        return Page();
    }
}
