using EduTrack.Application.Interfaces;
using EduTrack.Domain.Entities;
using EduTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Infrastructure.Repositories;

public sealed class ClassRepository : IClassRepository
{
    private readonly AppDbContext _context;

    public ClassRepository(AppDbContext context) => _context = context;

    public async Task<(IReadOnlyList<ClassSection> Items, int TotalCount)> GetPagedAsync(
        string? search,
        int? courseId,
        int? teacherId,
        string? semester,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = _context.Classes
            .AsNoTracking()
            .Include(classSection => classSection.Course)
            .Include(classSection => classSection.Teacher)
            .Include(classSection => classSection.Enrollments)
            .AsSplitQuery()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(classSection =>
                classSection.ClassCode.Contains(search) ||
                (classSection.Course != null &&
                 (classSection.Course.CourseCode.Contains(search) ||
                  classSection.Course.Name.Contains(search))));
        }

        if (courseId.HasValue)
            query = query.Where(classSection => classSection.CourseId == courseId.Value);

        if (teacherId.HasValue)
            query = query.Where(classSection => classSection.TeacherId == teacherId.Value);

        if (!string.IsNullOrWhiteSpace(semester))
            query = query.Where(classSection => classSection.Semester == semester);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(classSection => classSection.Semester)
            .ThenBy(classSection => classSection.ClassCode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<ClassSection?> GetByIdAsync(
        int id,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        var query = _context.Classes
            .Include(classSection => classSection.Course)
            .Include(classSection => classSection.Teacher)
            .Include(classSection => classSection.Enrollments)
            .AsSplitQuery()
            .AsQueryable();

        if (!trackChanges)
            query = query.AsNoTracking();

        return await query.SingleOrDefaultAsync(
            classSection => classSection.Id == id,
            cancellationToken);
    }

    public Task<ClassSection?> GetWithRosterAsync(
        int id,
        CancellationToken cancellationToken)
        => _context.Classes
            .AsNoTracking()
            .Include(classSection => classSection.Teacher)
            .Include(classSection => classSection.Enrollments)
                .ThenInclude(enrollment => enrollment.Student)
            .AsSplitQuery()
            .SingleOrDefaultAsync(classSection => classSection.Id == id, cancellationToken);

    public Task<bool> CodeExistsAsync(
        string classCode,
        int? excludingId,
        CancellationToken cancellationToken)
        => _context.Classes.AnyAsync(
            classSection => classSection.ClassCode == classCode &&
                            (!excludingId.HasValue || classSection.Id != excludingId.Value),
            cancellationToken);

    public Task<bool> CourseExistsAsync(int courseId, CancellationToken cancellationToken)
        => _context.Courses.AnyAsync(course => course.Id == courseId, cancellationToken);

    public Task<bool> TeacherExistsAsync(int teacherId, CancellationToken cancellationToken)
        => _context.Teachers.AnyAsync(teacher => teacher.Id == teacherId, cancellationToken);

    public Task AddAsync(ClassSection classSection, CancellationToken cancellationToken)
        => _context.Classes.AddAsync(classSection, cancellationToken).AsTask();

    public void Remove(ClassSection classSection) => _context.Classes.Remove(classSection);

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
        => await _context.SaveChangesAsync(cancellationToken);
}
