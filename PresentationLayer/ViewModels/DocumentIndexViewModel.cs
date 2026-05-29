using DataAccessLayer.Models;

namespace PresentationLayer.ViewModels;

public class DocumentIndexViewModel
{
    public IEnumerable<Document> Documents { get; set; } = new List<Document>();
    public IEnumerable<Subject> Subjects { get; set; } = new List<Subject>();
    public int? SelectedSubjectId { get; set; }
    public string Filter { get; set; } = "all"; // "all" | "indexed" | "pending"
}
