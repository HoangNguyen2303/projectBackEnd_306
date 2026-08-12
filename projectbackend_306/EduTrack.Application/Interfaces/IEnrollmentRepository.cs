using EduTrack.Domain.Entities;

namespace EduTrack.Application.Interfaces;

public interface IEnrollmentRepository
{
    Task<(IReadOnlyList<Enrollment> Items, int TotalCount)> GetPagedAsync(
        int? studentId,
        int? classId,
        string? semester,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<Student?> GetStudentAsync(int studentId, CancellationToken cancellationToken);
    Task<Student?> GetStudentByUserIdAsync(string userId, CancellationToken cancellationToken);
    Task<ClassSection?> GetClassAsync(int classId, CancellationToken cancellationToken);
    Task<Enrollment?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Enrollment?> FindAsync(int studentId, int classId, CancellationToken cancellationToken);
    Task AddAsync(Enrollment enrollment, CancellationToken cancellationToken);
    void Remove(Enrollment enrollment);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
