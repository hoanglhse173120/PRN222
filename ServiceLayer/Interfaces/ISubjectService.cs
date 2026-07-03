using ServiceLayer.DTOs;

namespace ServiceLayer.Interfaces;

public interface ISubjectService
{
    Task<IEnumerable<SubjectDto>> GetAllAsync();
    Task<SubjectDto?> GetByIdAsync(int id);
    Task CreateAsync(SubjectDto subjectDto);
    Task UpdateAsync(SubjectDto subjectDto);
    Task DeleteAsync(int id);
    Task<List<int>> GetAssignedSubjectIdsAsync(string teacherId);
    Task<List<string>> GetAssignedSubjectNamesAsync(string teacherId);
    Task<List<int>> GetTakenByOthersSubjectIdsAsync(string teacherId);
    Task RemoveAllAssignmentsAsync(string teacherId);
    Task<(bool Success, string ErrorMessage)> AssignSubjectsToTeacherAsync(string teacherId, List<int> subjectIds);
}
