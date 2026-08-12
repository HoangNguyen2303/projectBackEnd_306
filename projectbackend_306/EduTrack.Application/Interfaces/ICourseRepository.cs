using EduTrack.Domain.Entities;

namespace EduTrack.Application.Interfaces;

public interface ICourseRepository
{
    Task<(IReadOnlyList<Course> Items, int TotalCount)> GetPagedAsync(
        string? search,
        int? departmentId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<Course?> GetByIdAsync(
        int id,
        bool trackChanges,
        CancellationToken cancellationToken);

    Task<bool> CodeExistsAsync(
        string courseCode,
        int? excludingId,
        CancellationToken cancellationToken);

    Task<bool> DepartmentExistsAsync(int departmentId, CancellationToken cancellationToken);
    Task<bool> HasClassesAsync(int courseId, CancellationToken cancellationToken);
    Task AddAsync(Course course, CancellationToken cancellationToken);
    void Remove(Course course);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
