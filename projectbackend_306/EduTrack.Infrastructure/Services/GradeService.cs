using EduTrack.Application.Common;
using EduTrack.Application.DTOs;
using EduTrack.Application.Interfaces;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using EduTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Infrastructure.Services;

public class GradeService : IGradeService
{
    private readonly AppDbContext _context;

    public GradeService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<GradeDto>> GetAllAsync()
    {
        return await BaseQuery()
            .OrderByDescending(g => g.Id)
            .Select(g => MapToDto(g))
            .ToListAsync();
    }

    public async Task<List<GradeDto>> GetByUserIdAsync(string userId)
    {
        return await BaseQuery()
            .Where(g => g.Enrollment!.Student!.UserId == userId)
            .OrderByDescending(g => g.Id)
            .Select(g => MapToDto(g))
            .ToListAsync();
    }

    public async Task<GradeDto> GetByIdAsync(int gradeId, string requestingUserId, bool isPrivileged)
    {
        var grade = await BaseQuery().FirstOrDefaultAsync(g => g.Id == gradeId);
        if (grade == null)
            throw new NotFoundException($"Không tìm thấy điểm với id {gradeId}");

        // Defense-in-depth: service tự kiểm tra lại quyền sở hữu, không chỉ dựa vào
        // [Authorize] ở controller.
        if (!isPrivileged && grade.Enrollment!.Student!.UserId != requestingUserId)
            throw new ForbiddenException("Bạn không có quyền xem điểm này");

        return MapToDto(grade);
    }

    public async Task<GradeDto> CreateAsync(CreateGradeDto dto, string performedByUserId, bool isAdmin)
    {
        var enrollment = await _context.Enrollments
            .Include(e => e.Class)
            .FirstOrDefaultAsync(e => e.Id == dto.EnrollmentId);
        if (enrollment == null)
            throw new NotFoundException($"Không tìm thấy Enrollment với id {dto.EnrollmentId}");

        if (!isAdmin)
            await EnsureTeacherOwnsClassAsync(enrollment.ClassId, performedByUserId);

        // Quan hệ Enrollment-Grade là 1-1 (unique index EnrollmentId trong AppDbContext):
        // chặn tạo điểm thứ 2 cho cùng 1 enrollment.
        var alreadyHasGrade = await _context.Grades.AnyAsync(g => g.EnrollmentId == dto.EnrollmentId);
        if (alreadyHasGrade)
            throw new InvalidOperationException($"Enrollment {dto.EnrollmentId} đã có điểm, dùng PUT để sửa thay vì tạo mới");

        var grade = new Grade
        {
            EnrollmentId = dto.EnrollmentId,
            Attendance = dto.Attendance,
            Midterm = dto.Midterm,
            Final = dto.Final
        };
        ApplyComputedFields(grade);

        _context.Grades.Add(grade);
        await _context.SaveChangesAsync(); // lưu trước để EF gán Id, AuditLog cần Id thật

        AddAuditLog(
            userId: performedByUserId,
            action: "GRADE_CREATE",
            entity: $"Grade#{grade.Id}",
            oldValue: null,
            newValue: $"Attendance={grade.Attendance}, Midterm={grade.Midterm}, Final={grade.Final}, Total={grade.Total}, Classification={grade.Classification}");
        await _context.SaveChangesAsync();

        return await GetByIdAsync(grade.Id, performedByUserId, isPrivileged: true);
    }

    public async Task<GradeDto> UpdateAsync(int gradeId, UpdateGradeDto dto, string performedByUserId, bool isAdmin)
    {
        var grade = await _context.Grades
            .Include(g => g.Enrollment)
            .FirstOrDefaultAsync(g => g.Id == gradeId);
        if (grade == null)
            throw new NotFoundException($"Không tìm thấy điểm với id {gradeId}");

        if (!isAdmin)
            await EnsureTeacherOwnsClassAsync(grade.Enrollment!.ClassId, performedByUserId);

        var oldValue = $"Attendance={grade.Attendance}, Midterm={grade.Midterm}, Final={grade.Final}, Total={grade.Total}, Classification={grade.Classification}";

        grade.Attendance = dto.Attendance;
        grade.Midterm = dto.Midterm;
        grade.Final = dto.Final;
        ApplyComputedFields(grade);

        AddAuditLog(
            userId: performedByUserId,
            action: "GRADE_UPDATE",
            entity: $"Grade#{grade.Id}",
            oldValue: oldValue,
            newValue: $"Attendance={grade.Attendance}, Midterm={grade.Midterm}, Final={grade.Final}, Total={grade.Total}, Classification={grade.Classification}");

        await _context.SaveChangesAsync();

        return await GetByIdAsync(grade.Id, performedByUserId, isPrivileged: true);
    }

    public async Task DeleteAsync(int gradeId, string performedByUserId, bool isAdmin)
    {
        var grade = await _context.Grades
            .Include(g => g.Enrollment)
            .FirstOrDefaultAsync(g => g.Id == gradeId);
        if (grade == null)
            throw new NotFoundException($"Không tìm thấy điểm với id {gradeId}");

        if (!isAdmin)
            await EnsureTeacherOwnsClassAsync(grade.Enrollment!.ClassId, performedByUserId);

        AddAuditLog(
            userId: performedByUserId,
            action: "GRADE_DELETE",
            entity: $"Grade#{grade.Id}",
            oldValue: $"Attendance={grade.Attendance}, Midterm={grade.Midterm}, Final={grade.Final}, Total={grade.Total}, Classification={grade.Classification}",
            newValue: null);

        _context.Grades.Remove(grade);
        await _context.SaveChangesAsync();
    }

    public async Task<List<GradeDto>> GetGradesByStudentAsync(int studentId, string requestingUserId, bool isPrivileged)
    {
        await GetStudentOrThrowAsync(studentId, requestingUserId, isPrivileged);

        return await BaseQuery()
            .Where(g => g.Enrollment!.StudentId == studentId)
            .OrderByDescending(g => g.Id)
            .Select(g => MapToDto(g))
            .ToListAsync();
    }

    public async Task<GpaDto> GetGpaByStudentAsync(int studentId, string requestingUserId, bool isPrivileged)
    {
        await GetStudentOrThrowAsync(studentId, requestingUserId, isPrivileged);

        // §10.3: GPA = Σ(Course.Credits × Total) / Σ(Course.Credits) — join Grade -> Enrollment -> ClassSection -> Course.
        var rows = await _context.Grades
            .Where(g => g.Enrollment!.StudentId == studentId)
            .Select(g => new { g.Total, Credits = g.Enrollment!.Class!.Course!.Credits })
            .ToListAsync();

        var totalCredits = rows.Sum(r => r.Credits);
        var gpa = totalCredits == 0
            ? 0m
            : Math.Round(rows.Sum(r => r.Total * r.Credits) / totalCredits, 2);

        return new GpaDto
        {
            StudentId = studentId,
            Gpa = gpa,
            Classification = ClassifyGpa(gpa).ToString()
        };
    }

    // ----------------- helpers -----------------

    /// <summary>Kiểm tra tồn tại + quyền sở hữu, dùng chung cho GetGradesByStudentAsync/GetGpaByStudentAsync
    /// — cùng kiểu Admin/Teacher xem ai cũng được, Student chỉ xem của chính mình như GetByIdAsync.</summary>
    private async Task<Student> GetStudentOrThrowAsync(int studentId, string requestingUserId, bool isPrivileged)
    {
        var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == studentId);
        if (student == null)
            throw new NotFoundException($"Không tìm thấy sinh viên với id {studentId}");

        if (!isPrivileged && student.UserId != requestingUserId)
            throw new ForbiddenException("Bạn không có quyền xem dữ liệu của sinh viên này");

        return student;
    }

    private static Classification ClassifyGpa(decimal gpa) => gpa switch
    {
        >= 8.0m => Classification.Gioi,
        >= 6.5m => Classification.Kha,
        >= 5.0m => Classification.TrungBinh,
        _ => Classification.Yeu
    };

    private IQueryable<Grade> BaseQuery()
    {
        return _context.Grades
            .Include(g => g.Enrollment)
                .ThenInclude(e => e!.Student)
            .Include(g => g.Enrollment)
                .ThenInclude(e => e!.Class);
    }

    private static GradeDto MapToDto(Grade g)
    {
        return new GradeDto
        {
            Id = g.Id,
            EnrollmentId = g.EnrollmentId,
            StudentId = g.Enrollment!.StudentId,
            StudentName = g.Enrollment.Student?.FullName,
            ClassId = g.Enrollment.ClassId,
            ClassCode = g.Enrollment.Class?.ClassCode,
            Attendance = g.Attendance,
            Midterm = g.Midterm,
            Final = g.Final,
            Total = g.Total,
            Classification = g.Classification.ToString()
        };
    }

    /// <summary>Công thức + ngưỡng phân loại theo đúng project doc §10.3.</summary>
    private static void ApplyComputedFields(Grade grade)
    {
        grade.Total = Math.Round(grade.Attendance * 0.1m + grade.Midterm * 0.3m + grade.Final * 0.6m, 2);
        grade.Classification = ClassifyGpa(grade.Total);
    }

    /// <summary>§10.3: chỉ giáo viên phụ trách đúng lớp (qua ClassSection.TeacherId) hoặc Admin mới được sửa điểm.</summary>
    private async Task EnsureTeacherOwnsClassAsync(int classId, string performedByUserId)
    {
        var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == performedByUserId);
        if (teacher == null)
            throw new ForbiddenException("Tài khoản này không gắn với hồ sơ giáo viên nào");

        var classSection = await _context.Classes.FirstOrDefaultAsync(c => c.Id == classId);
        if (classSection == null || classSection.TeacherId != teacher.Id)
            throw new ForbiddenException("Bạn không phải giáo viên phụ trách lớp này");
    }

    private void AddAuditLog(string userId, string action, string entity, string? oldValue, string? newValue)
    {
        _context.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = action,
            Entity = entity,
            OldValue = oldValue,
            NewValue = newValue,
            At = DateTime.UtcNow
        });
    }
}
