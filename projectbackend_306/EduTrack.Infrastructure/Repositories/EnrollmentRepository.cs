using EduTrack.Application.Interfaces;
using EduTrack.Domain.Entities;
using EduTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Infrastructure.Repositories;

public sealed class EnrollmentRepository : IEnrollmentRepository
{
    private readonly AppDbContext _context;

    public EnrollmentRepository(AppDbContext context) => _context = context;

    public async Task<(IReadOnlyList<Enrollment> Items, int TotalCount)> GetPagedAsync(
        int? studentId,
        int? classId,
        string? semester,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = _context.Enrollments
            .AsNoTracking()
            .Include(enrollment => enrollment.Student)
            .Include(enrollment => enrollment.Class)
                .ThenInclude(classSection => classSection!.Course)
            .AsSplitQuery()
            .AsQueryable();

        if (studentId.HasValue)
            query = query.Where(enrollment => enrollment.StudentId == studentId.Value);

        if (classId.HasValue)
            query = query.Where(enrollment => enrollment.ClassId == classId.Value);

        if (!string.IsNullOrWhiteSpace(semester))
        {
            query = query.Where(enrollment =>
                enrollment.Class != null && enrollment.Class.Semester == semester);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(enrollment => enrollment.EnrolledAt)
            .ThenBy(enrollment => enrollment.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<Student?> GetStudentAsync(int studentId, CancellationToken cancellationToken)
        => _context.Students.SingleOrDefaultAsync(
            student => student.Id == studentId,
            cancellationToken);

    public Task<Student?> GetStudentByUserIdAsync(
        string userId,
        CancellationToken cancellationToken)
        => _context.Students
            .AsNoTracking()
            .SingleOrDefaultAsync(student => student.UserId == userId, cancellationToken);

    public Task<ClassSection?> GetClassAsync(int classId, CancellationToken cancellationToken)
        => _context.Classes
            .Include(classSection => classSection.Course)
            .Include(classSection => classSection.Enrollments)
            .SingleOrDefaultAsync(classSection => classSection.Id == classId, cancellationToken);

    public Task<Enrollment?> GetByIdAsync(int id, CancellationToken cancellationToken)
        => _context.Enrollments
            .Include(enrollment => enrollment.Student)
            .Include(enrollment => enrollment.Class)
                .ThenInclude(classSection => classSection!.Course)
            .SingleOrDefaultAsync(enrollment => enrollment.Id == id, cancellationToken);

    public Task<Enrollment?> FindAsync(
        int studentId,
        int classId,
        CancellationToken cancellationToken)
        => _context.Enrollments.SingleOrDefaultAsync(
            enrollment => enrollment.StudentId == studentId &&
                          enrollment.ClassId == classId,
            cancellationToken);

    public Task AddAsync(Enrollment enrollment, CancellationToken cancellationToken)
        => _context.Enrollments.AddAsync(enrollment, cancellationToken).AsTask();

    public void Remove(Enrollment enrollment) => _context.Enrollments.Remove(enrollment);

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
        => await _context.SaveChangesAsync(cancellationToken);
}
