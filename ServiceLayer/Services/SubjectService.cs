using DataAccessLayer.Models;
using DataAccessLayer.Repositories;
using ServiceLayer.Interfaces;

namespace ServiceLayer.Services;

public class SubjectService : ISubjectService
{
    private readonly IRepository<Subject> _repo;

    public SubjectService(IRepository<Subject> repo) => _repo = repo;

    public async Task<IEnumerable<Subject>> GetAllAsync() => await _repo.GetAllAsync();
    public async Task<Subject?> GetByIdAsync(int id) => await _repo.GetByIdAsync(id);

    public async Task CreateAsync(Subject subject)
    {
        await _repo.AddAsync(subject);
        await _repo.SaveChangesAsync();
    }

    public async Task UpdateAsync(Subject subject)
    {
        _repo.Update(subject);
        await _repo.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity != null)
        {
            _repo.Delete(entity);
            await _repo.SaveChangesAsync();
        }
    }
}
