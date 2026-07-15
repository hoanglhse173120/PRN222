using ServiceLayer.DTOs;

namespace PresentationLayer.ViewModels;

public class DocumentIndexViewModel
{
    public IEnumerable<DocumentDto> Documents { get; set; } = new List<DocumentDto>();
    public IEnumerable<SubjectDto> Subjects { get; set; } = new List<SubjectDto>();
    public int? SelectedSubjectId { get; set; }
}
