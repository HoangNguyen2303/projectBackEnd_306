using EduTrack.Domain.Entities;

namespace EduTrack.Application.Interfaces;

public interface IDepartmentRepository
{
    Task<(IReadOnlyList<Department> Items, int TotalCount)> GetPagedAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<Department?> GetByIdAsync(
        int id,
        bool trackChanges,
        CancellationToken cancellationToken);

    Task<bool> CodeExistsAsync(
        string code,
        int? excludingId,
        CancellationToken cancellationToken);

    Task<bool> HasRelatedDataAsync(int departmentId, CancellationToken cancellationToken);
    Task AddAsync(Department department, CancellationToken cancellationToken);
    void Remove(Department department);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
