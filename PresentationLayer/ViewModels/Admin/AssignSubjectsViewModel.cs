namespace PresentationLayer.ViewModels.Admin;

public class AssignSubjectsViewModel
{
    public string UserId { get; set; } = "";
    public string UserEmail { get; set; } = "";

    // Tất cả môn học trong hệ thống
    public List<SubjectOption> AllSubjects { get; set; } = new();

    // Danh sách SubjectId đã được phân công (tối đa 2)
    public List<int> AssignedSubjectIds { get; set; } = new();

    // SubjectId đã được phân cho giảng viên KHÁC (disable trong UI)
    public List<int> TakenByOtherIds { get; set; } = new();
}

public class SubjectOption
{
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = "";
}
