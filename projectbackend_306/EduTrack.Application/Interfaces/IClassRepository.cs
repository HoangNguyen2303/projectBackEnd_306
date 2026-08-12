using EduTrack.Domain.Entities;

namespace EduTrack.Application.Interfaces;

public interface IClassRepository
{
    Task<(IReadOnlyList<ClassSection> Items, int TotalCount)> GetPagedAsync(
        string? search,
        int? courseId,
        int? teacherId,
        string? semester,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<ClassSection?> GetByIdAsync(
        int id,
        bool trackChanges,
        CancellationToken cancellationToken);

    Task<ClassSection?> GetWithRosterAsync(int id, CancellationToken cancellationToken);
    Task<bool> CodeExistsAsync(string classCode, int? excludingId, CancellationToken cancellationToken);
    Task<bool> CourseExistsAsync(int courseId, CancellationToken cancellationToken);
    Task<bool> TeacherExistsAsync(int teacherId, CancellationToken cancellationToken);
    Task AddAsync(ClassSection classSection, CancellationToken cancellationToken);
    void Remove(ClassSection classSection);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
