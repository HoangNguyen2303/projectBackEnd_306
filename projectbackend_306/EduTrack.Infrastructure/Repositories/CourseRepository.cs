using EduTrack.Application.Interfaces;
using EduTrack.Domain.Entities;
using EduTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Infrastructure.Repositories;

public sealed class CourseRepository : ICourseRepository
{
    private readonly AppDbContext _context;

    public CourseRepository(AppDbContext context) => _context = context;

    public async Task<(IReadOnlyList<Course> Items, int TotalCount)> GetPagedAsync(
        string? search,
        int? departmentId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = _context.Courses
            .AsNoTracking()
            .Include(course => course.Department)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(course =>
                course.CourseCode.Contains(search) ||
                course.Name.Contains(search));
        }

        if (departmentId.HasValue)
            query = query.Where(course => course.DepartmentId == departmentId.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(course => course.CourseCode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Course?> GetByIdAsync(
        int id,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        var query = _context.Courses
            .Include(course => course.Department)
            .AsQueryable();

        if (!trackChanges)
            query = query.AsNoTracking();

        return await query.SingleOrDefaultAsync(course => course.Id == id, cancellationToken);
    }

    public Task<bool> CodeExistsAsync(
        string courseCode,
        int? excludingId,
        CancellationToken cancellationToken)
        => _context.Courses.AnyAsync(
            course => course.CourseCode == courseCode &&
                      (!excludingId.HasValue || course.Id != excludingId.Value),
            cancellationToken);

    public Task<bool> DepartmentExistsAsync(
        int departmentId,
        CancellationToken cancellationToken)
        => _context.Departments.AnyAsync(
            department => department.Id == departmentId,
            cancellationToken);

    public Task<bool> HasClassesAsync(int courseId, CancellationToken cancellationToken)
        => _context.Classes.AnyAsync(
            classSection => classSection.CourseId == courseId,
            cancellationToken);

    public Task AddAsync(Course course, CancellationToken cancellationToken)
        => _context.Courses.AddAsync(course, cancellationToken).AsTask();

    public void Remove(Course course) => _context.Courses.Remove(course);

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
        => await _context.SaveChangesAsync(cancellationToken);
}
