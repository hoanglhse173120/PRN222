using DataAccessLayer.Entities;
using DataAccessLayer.Repositories;
using ServiceLayer.Interfaces;
using ServiceLayer.DTOs;
using System.Linq;

namespace ServiceLayer.Services;

public class SubjectService : ISubjectService
{
    private readonly IRepository<Subject> _repo;

    public SubjectService(IRepository<Subject> repo) => _repo = repo;

    public async Task<IEnumerable<SubjectDto>> GetAllAsync()
    {
        var entities = await _repo.GetAllAsync();
        return entities.Select(e => new SubjectDto
        {
            SubjectID = e.SubjectId,
            SubjectName = e.SubjectName,
            Description = e.Description,
            CreatedAt = e.CreatedAt ?? DateTime.MinValue
        });
    }

    public async Task<SubjectDto?> GetByIdAsync(int id)
    {
        var e = await _repo.GetByIdAsync(id);
        if (e == null) return null;
        return new SubjectDto
        {
            SubjectID = e.SubjectId,
            SubjectName = e.SubjectName,
            Description = e.Description,
            CreatedAt = e.CreatedAt ?? DateTime.MinValue
        };
    }

    public async Task CreateAsync(SubjectDto dto)
    {
        var subject = new Subject
        {
            SubjectName = dto.SubjectName,
            Description = dto.Description,
            CreatedAt = DateTime.Now
        };
        await _repo.AddAsync(subject);
        await _repo.SaveChangesAsync();
    }

    public async Task UpdateAsync(SubjectDto dto)
    {
        var subject = await _repo.GetByIdAsync(dto.SubjectID);
        if (subject != null)
        {
            subject.SubjectName = dto.SubjectName;
            subject.Description = dto.Description;
            _repo.Update(subject);
            await _repo.SaveChangesAsync();
        }
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
