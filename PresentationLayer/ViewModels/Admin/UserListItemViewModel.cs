namespace PresentationLayer.ViewModels.Admin;

public class UserListItemViewModel
{
    public string UserId { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public List<string> AssignedSubjects { get; set; } = new();
}
