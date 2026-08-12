using EduTrack.Application.DTOs.Auth.Teacher;
using EduTrack.Application.Interfaces;
using EduTrack.Domain.Entities;
using EduTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Infrastructure.Repositories;

public sealed class TeacherRepository : ITeacherRepository
{
    private readonly AppDbContext _context;

    public TeacherRepository(AppDbContext context) => _context = context;

    public async Task<List<Teacher>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Teachers
            .Include(t => t.Department) // 💡 Nạp thông tin Department để không bị null
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Teacher?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Teachers
            .Include(t => t.Department) // 💡 Nạp thông tin Department để không bị null
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task AddAsync(Teacher teacher, CancellationToken cancellationToken = default)
    {
        await _context.Teachers.AddAsync(teacher, cancellationToken);
    }

    public void Remove(Teacher teacher)
    {
        _context.Teachers.Remove(teacher);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
    // kiểm trả department khi nhập
    public async Task<bool> DepartmentExistsAsync(int departmentId, CancellationToken cancellationToken = default)
    {
        return await _context.Departments.AnyAsync(d => d.Id == departmentId, cancellationToken);
    }
    public async Task<(IReadOnlyList<Teacher> Items, int TotalCount)> GetPagedAsync(
        TeacherQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var dbQuery = _context.Teachers
            .Include(t => t.Department)
            .AsNoTracking()
            .AsQueryable();

        // 1. Lọc theo từ khóa (Tìm theo Họ tên, Email hoặc Mã giảng viên)
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            dbQuery = dbQuery.Where(t =>
                t.FullName.Contains(query.Search) ||
                t.Email.Contains(query.Search) ||
                t.TeacherCode.Contains(query.Search));
        }

        // 2. Đếm tổng số lượng giảng viên thỏa điều kiện
        var totalCount = await dbQuery.CountAsync(cancellationToken);

        // 3. Cắt dữ liệu theo Page và PageSize
        var items = await dbQuery
            .OrderBy(t => t.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}