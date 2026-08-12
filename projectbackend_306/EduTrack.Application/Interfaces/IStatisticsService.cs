using EduTrack.Application.DTOs;

namespace EduTrack.Application.Interfaces;

public interface IStatisticsService
{
    Task<List<StudentsByDepartmentDto>> GetStudentsByDepartmentAsync();
    Task<List<PassFailStatsDto>> GetPassFailAsync(string? semester = null);
    Task<List<TopStudentDto>> GetTopStudentsAsync(int topN = 10, string? semester = null);
    Task<List<AcademicWarningDto>> GetAcademicWarningsAsync(decimal gpaThreshold = 5.0m);
    Task<List<AuditLogDto>> GetAuditLogsAsync(DateTime? from = null, DateTime? to = null);
}
