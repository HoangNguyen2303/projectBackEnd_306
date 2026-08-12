using EduTrack.Application.DTOs;
using EduTrack.Application.Interfaces;
using EduTrack.Domain.Enums;
using EduTrack.Infrastructure.Data;
using EduTrack.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Infrastructure.Services;

public class StatisticsService : IStatisticsService
{
    private readonly AppDbContext _context;

    public StatisticsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<StudentsByDepartmentDto>> GetStudentsByDepartmentAsync()
    {
        var query =
            from d in _context.Departments
            join s in _context.Students on d.Id equals s.DepartmentId into studentsGroup
            from sg in studentsGroup.DefaultIfEmpty()
            group sg by new { d.Id, d.Code, d.Name } into g
            select new StudentsByDepartmentDto
            {
                DepartmentId = g.Key.Id,
                DepartmentCode = g.Key.Code,
                DepartmentName = g.Key.Name,
                StudentCount = g.Count(x => x != null)
            };

        return await query.OrderByDescending(x => x.StudentCount).ToListAsync();
    }

    public async Task<List<PassFailStatsDto>> GetPassFailAsync(string? semester = null)
    {
        var baseQuery =
            from e in _context.Enrollments
            join c in _context.Classes on e.ClassId equals c.Id
            join g in _context.Grades on e.Id equals g.EnrollmentId into gradeGroup
            from gg in gradeGroup.DefaultIfEmpty()
            where semester == null || c.Semester == semester
            select new
            {
                c.Semester,
                Total = gg != null ? gg.Total : (decimal?)null
            };

        var raw = await baseQuery.ToListAsync();

        var stats =
            from q in raw
            group q by q.Semester into g
            let total = g.Count()
            let passed = g.Count(x => x.Total >= 5.0m)
            let failed = g.Count(x => x.Total < 5.0m)
            orderby g.Key
            select new PassFailStatsDto
            {
                Semester = g.Key,
                TotalEnrollments = total,
                PassedCount = passed,
                FailedCount = failed,
                PassPercentage = total == 0 ? 0 : Math.Round((double)passed / total * 100, 2),
                FailPercentage = total == 0 ? 0 : Math.Round((double)failed / total * 100, 2)
            };

        return stats.ToList();
    }

    public async Task<List<TopStudentDto>> GetTopStudentsAsync(int topN = 10, string? semester = null)
    {
        var gradeQuery =
            from e in _context.Enrollments
            join c in _context.Classes on e.ClassId equals c.Id
            join g in _context.Grades on e.Id equals g.EnrollmentId
            join co in _context.Courses on c.CourseId equals co.Id
            where semester == null || c.Semester == semester
            select new { e.StudentId, g.Total, co.Credits };

        var rawGrades = await gradeQuery.ToListAsync();

        var gpaList =
            from q in rawGrades
            group q by q.StudentId into g
            let totalCredits = g.Sum(x => x.Credits)
            let weightedSum = g.Sum(x => x.Credits * x.Total)
            let gpa = totalCredits == 0 ? 0 : Math.Round(weightedSum / totalCredits, 2)
            select new { StudentId = g.Key, Gpa = gpa };

        var studentIds = gpaList.Select(x => x.StudentId).ToList();
        var students = await _context.Students
            .Where(s => studentIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id);

        var result =
            from gq in gpaList
            join s in students on gq.StudentId equals s.Key into sJoin
            from sj in sJoin.DefaultIfEmpty()
            let name = sj.Value != null ? sj.Value.FullName : string.Empty
            let code = sj.Value != null ? sj.Value.StudentCode : string.Empty
            orderby gq.Gpa descending
            select new TopStudentDto
            {
                StudentId = gq.StudentId,
                StudentCode = code,
                FullName = name,
                Gpa = gq.Gpa,
                Classification = GetClassificationText(gq.Gpa)
            };

        return result.Take(topN).ToList();
    }

    public async Task<List<AcademicWarningDto>> GetAcademicWarningsAsync(decimal gpaThreshold = 5.0m)
    {
        var gradeQuery =
            from e in _context.Enrollments
            join c in _context.Classes on e.ClassId equals c.Id
            join g in _context.Grades on e.Id equals g.EnrollmentId
            join co in _context.Courses on c.CourseId equals co.Id
            select new { e.StudentId, g.Total, co.Credits, Passed = g.Total >= 5.0m };

        var rawGrades = await gradeQuery.ToListAsync();

        var summaryList =
            from q in rawGrades
            group q by q.StudentId into g
            let totalCredits = g.Sum(x => x.Credits)
            let weightedSum = g.Sum(x => x.Credits * x.Total)
            let gpa = totalCredits == 0 ? 0 : Math.Round(weightedSum / totalCredits, 2)
            let failedCount = g.Count(x => !x.Passed)
            where gpa < gpaThreshold || failedCount >= 1
            select new { StudentId = g.Key, Gpa = gpa, FailedCount = failedCount };

        var studentIds = summaryList.Select(x => x.StudentId).ToList();
        var students = await _context.Students
            .Include(s => s.Department)
            .Where(s => studentIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id);

        var result =
            from sq in summaryList
            join s in students on sq.StudentId equals s.Key into sJoin
            from sj in sJoin.DefaultIfEmpty()
            let st = sj.Value
            let deptName = st?.Department?.Name ?? string.Empty
            let warningLevel =
                sq.FailedCount >= 2 ? "Cảnh báo mức 2 - Thiếu hụt nhiều môn" :
                sq.Gpa < 5.0m ? "Cảnh báo mức 1 - GPA dưới ngưỡng" :
                "Cảnh báo - Đã có môn rớt"
            orderby sq.Gpa ascending
            select new AcademicWarningDto
            {
                StudentId = sq.StudentId,
                StudentCode = st?.StudentCode ?? string.Empty,
                FullName = st?.FullName ?? string.Empty,
                DepartmentName = deptName,
                Gpa = sq.Gpa,
                Classification = GetClassificationText(sq.Gpa),
                FailedCourseCount = sq.FailedCount,
                WarningLevel = warningLevel
            };

        return result.ToList();
    }

    public async Task<List<AuditLogDto>> GetAuditLogsAsync(DateTime? from = null, DateTime? to = null)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (from.HasValue)
            query = query.Where(x => x.At >= from.Value);
        if (to.HasValue)
            query = query.Where(x => x.At <= to.Value);

        var resultQuery =
            from a in query
            join u in _context.Users on a.UserId equals u.Id into userJoin
            from u in userJoin.DefaultIfEmpty()
            orderby a.At descending
            select new AuditLogDto
            {
                Id = a.Id,
                UserId = a.UserId,
                UserEmail = u != null ? (u.Email ?? string.Empty) : string.Empty,
                Action = a.Action,
                Entity = a.Entity,
                OldValue = a.OldValue,
                NewValue = a.NewValue,
                At = a.At
            };

        return await resultQuery.ToListAsync();
    }

    private static string GetClassificationText(decimal gpa)
    {
        return gpa switch
        {
            >= 8.0m => "Giỏi",
            >= 6.5m => "Khá",
            >= 5.0m => "Trung bình",
            _ => "Yếu"
        };
    }
}
