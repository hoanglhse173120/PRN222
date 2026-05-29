using DataAccessLayer.Models;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace PresentationLayer.ViewModels;

public class DocumentUploadViewModel
{
    [Required(ErrorMessage = "Vui lòng chọn môn học")]
    public int SubjectId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn file")]
    public IFormFile File { get; set; } = null!;

    public IEnumerable<Subject> Subjects { get; set; } = new List<Subject>();
}
