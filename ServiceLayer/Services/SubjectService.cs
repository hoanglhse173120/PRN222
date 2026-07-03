using DataAccessLayer.Entities;
using DataAccessLayer.Repositories;
using ServiceLayer.Interfaces;
using ServiceLayer.DTOs;
using System.Linq;

namespace ServiceLayer.Services;

public class SubjectService : ISubjectService
{
    private readonly IRepository<Subject> _repo;
    private readonly IRepository<TeacherSubject> _tsRepo;

    public SubjectService(IRepository<Subject> repo, IRepository<TeacherSubject> tsRepo)
    {
        _repo = repo;
        _tsRepo = tsRepo;
    }

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

    public async Task<List<int>> GetAssignedSubjectIdsAsync(string teacherId)
    {
        var assigned = await _tsRepo.FindAsync(ts => ts.TeacherId == teacherId);
        return assigned.Select(ts => ts.SubjectId).ToList();
    }

    public async Task<List<string>> GetAssignedSubjectNamesAsync(string teacherId)
    {
        var assigned = await _tsRepo.FindWithIncludesAsync(
            ts => ts.TeacherId == teacherId,
            ts => ts.Subject
        );
        return assigned.Select(ts => ts.Subject.SubjectName).ToList();
    }

    public async Task<List<int>> GetTakenByOthersSubjectIdsAsync(string teacherId)
    {
        var taken = await _tsRepo.FindAsync(ts => ts.TeacherId != teacherId);
        return taken.Select(ts => ts.SubjectId).ToList();
    }

    public async Task RemoveAllAssignmentsAsync(string teacherId)
    {
        var assignments = await _tsRepo.FindAsync(ts => ts.TeacherId == teacherId);
        foreach (var assignment in assignments)
        {
            _tsRepo.Delete(assignment);
        }
        await _tsRepo.SaveChangesAsync();
    }

    public async Task<(bool Success, string ErrorMessage)> AssignSubjectsToTeacherAsync(string teacherId, List<int> subjectIds)
    {
        subjectIds = subjectIds.Distinct().ToList();
        
        if (subjectIds.Count > 2)
        {
            return (false, "Giảng viên chỉ được phân công tối đa 2 môn học.");
        }

        var conflicts = await _tsRepo.FindAsync(ts => ts.TeacherId != teacherId && subjectIds.Contains(ts.SubjectId));
        if (conflicts.Any())
        {
            var conflictSubjectIds = conflicts.Select(c => c.SubjectId).Distinct().ToList();
            var conflictSubjects = await _repo.FindAsync(s => conflictSubjectIds.Contains(s.SubjectId));
            var names = string.Join(", ", conflictSubjects.Select(s => $"\"{s.SubjectName}\""));
            return (false, $"Không thể phân công! Các môn {names} đã được phân cho giảng viên khác.");
        }

        var existing = await _tsRepo.FindAsync(ts => ts.TeacherId == teacherId);
        foreach (var assignment in existing)
        {
            _tsRepo.Delete(assignment);
        }

        foreach (var subjectId in subjectIds)
        {
            await _tsRepo.AddAsync(new TeacherSubject
            {
                TeacherId = teacherId,
                SubjectId = subjectId,
                AssignedAt = DateTime.Now
            });
        }

        await _tsRepo.SaveChangesAsync();
        return (true, string.Empty);
    }
}
