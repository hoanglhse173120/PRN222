using DataAccessLayer.Models;

namespace ServiceLayer.Interfaces;

public interface ISubjectService
{
    Task<IEnumerable<Subject>> GetAllAsync();
    Task<Subject?> GetByIdAsync(int id);
    Task CreateAsync(Subject subject);
    Task UpdateAsync(Subject subject);
    Task DeleteAsync(int id);
}
