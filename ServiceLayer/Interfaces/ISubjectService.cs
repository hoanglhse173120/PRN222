using DataAccessLayer.Entities;
using ServiceLayer.DTOs;

namespace ServiceLayer.Interfaces;

public interface ISubjectService
{
    Task<IEnumerable<SubjectDto>> GetAllAsync();
    Task<SubjectDto?> GetByIdAsync(int id);
    Task CreateAsync(SubjectDto subjectDto);
    Task UpdateAsync(SubjectDto subjectDto);
    Task DeleteAsync(int id);
}
