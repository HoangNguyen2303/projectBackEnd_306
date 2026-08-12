using EduTrack.Application.Interfaces;
using EduTrack.Domain.Entities;
using EduTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Infrastructure.Repositories;

public sealed class DepartmentRepository : IDepartmentRepository
{
    private readonly AppDbContext _context;

    public DepartmentRepository(AppDbContext context) => _context = context;

    public async Task<(IReadOnlyList<Department> Items, int TotalCount)> GetPagedAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = WithRelatedCounts(_context.Departments.AsNoTracking());

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(department =>
                department.Code.Contains(search) || department.Name.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(department => department.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Department?> GetByIdAsync(
        int id,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        IQueryable<Department> query = _context.Departments;
        if (!trackChanges)
            query = query.AsNoTracking();

        query = WithRelatedCounts(query);
        return await query.SingleOrDefaultAsync(
            department => department.Id == id,
            cancellationToken);
    }

    public Task<bool> CodeExistsAsync(
        string code,
        int? excludingId,
        CancellationToken cancellationToken)
        => _context.Departments.AnyAsync(
            department => department.Code == code &&
                          (!excludingId.HasValue || department.Id != excludingId.Value),
            cancellationToken);

    public async Task<bool> HasRelatedDataAsync(
        int departmentId,
        CancellationToken cancellationToken)
        => await _context.Students.AnyAsync(
               student => student.DepartmentId == departmentId,
               cancellationToken) ||
           await _context.Teachers.AnyAsync(
               teacher => teacher.DepartmentId == departmentId,
               cancellationToken) ||
           await _context.Courses.AnyAsync(
               course => course.DepartmentId == departmentId,
               cancellationToken);

    public Task AddAsync(Department department, CancellationToken cancellationToken)
        => _context.Departments.AddAsync(department, cancellationToken).AsTask();

    public void Remove(Department department) => _context.Departments.Remove(department);

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
        => await _context.SaveChangesAsync(cancellationToken);

    private static IQueryable<Department> WithRelatedCounts(IQueryable<Department> query)
        => query
            .Include(department => department.Students)
            .Include(department => department.Teachers)
            .Include(department => department.Courses)
            .AsSplitQuery();
}
