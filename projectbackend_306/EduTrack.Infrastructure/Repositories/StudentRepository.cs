using EduTrack.Application.DTOs.Auth.Student;
using EduTrack.Application.Interfaces;
using EduTrack.Domain.Entities;
using EduTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Infrastructure.Repositories;

public sealed class StudentRepository : IStudentRepository
{
    private readonly AppDbContext _context;

    public StudentRepository(AppDbContext context) => _context = context;

    public async Task<List<Student>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task AddAsync(Student student, CancellationToken cancellationToken = default)
    {
        await _context.Students.AddAsync(student, cancellationToken);
    }

    public void Remove(Student student)
    {
        _context.Students.Remove(student);
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
    // kiểm tra Dob khi nhập
    public async Task<bool> CodeExistsAsync(string studentCode, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .AnyAsync(s => s.StudentCode == studentCode && s.Id != excludeId, cancellationToken);
    }
    //kiểm tra trùng Email
    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        var cleanEmail = email.Trim().ToLower();
        return await _context.Students
            .AnyAsync(s => s.Email.ToLower() == cleanEmail, cancellationToken);
    }
    // Phân trang
    public async Task<(IReadOnlyList<Student> Items, int TotalCount)> GetPagedAsync(
    StudentQueryDto query,
    CancellationToken cancellationToken = default)
    {
        var dbQuery = _context.Students
         .Include(s => s.Department) // Nạp sẵn thông tin Khoa
         .AsNoTracking()
         .AsQueryable();

        // 1. Lọc theo từ khóa (nếu người dùng gõ tìm kiếm)
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var searchTerm = query.Search.Trim().ToLower();
            dbQuery = dbQuery.Where(s =>
                s.FullName.ToLower().Contains(searchTerm) ||
                s.Email.ToLower().Contains(searchTerm));
        }
        if (query.DepartmentId.HasValue)
        {
            dbQuery = dbQuery.Where(s => s.DepartmentId == query.DepartmentId.Value);
        }
        // CHỨC NĂNG SORTING (SẮP XẾP DỮ LIỆU)
        dbQuery = query.SortBy?.ToLower() switch
        {
            "fullname" or "name" => query.IsDescending
                ? dbQuery.OrderByDescending(s => s.FullName)
                : dbQuery.OrderBy(s => s.FullName),

            "email" => query.IsDescending
                ? dbQuery.OrderByDescending(s => s.Email)
                : dbQuery.OrderBy(s => s.Email),

            _ => query.IsDescending
                ? dbQuery.OrderByDescending(s => s.Id)
                : dbQuery.OrderBy(s => s.Id) // Mặc định xếp theo Id
        };

        // 2. Đếm tổng số lượng học sinh thỏa điều kiện
        var totalCount = await dbQuery.CountAsync(cancellationToken);

        // 3. Cắt dữ liệu theo Page và PageSize
        var items = await dbQuery
            .OrderBy(s => s.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}