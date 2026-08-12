using EduTrack.Application.Common;
using EduTrack.Application.DTOs;
using EduTrack.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.Api.Controllers;

[ApiController]
[Route("api/stats")]
[Authorize(Roles = "Admin,Teacher")]
public class StatsController : ControllerBase
{
    private readonly IStatisticsService _statsService;

    public StatsController(IStatisticsService statsService)
    {
        _statsService = statsService;
    }

    /// <summary>Thống kê số lượng sinh viên theo từng khoa/viện.</summary>
    [HttpGet("students-by-department")]
    public async Task<ActionResult<ApiResponse<List<StudentsByDepartmentDto>>>> GetStudentsByDepartment()
    {
        var result = await _statsService.GetStudentsByDepartmentAsync();
        return Ok(ApiResponse<List<StudentsByDepartmentDto>>.Ok(result));
    }

    /// <summary>Thống kê tỷ lệ đậu/rớt theo học kỳ (hoặc tất cả học kỳ).</summary>
    [HttpGet("pass-fail")]
    public async Task<ActionResult<ApiResponse<List<PassFailStatsDto>>>> GetPassFail([FromQuery] string? semester = null)
    {
        var result = await _statsService.GetPassFailAsync(semester);
        return Ok(ApiResponse<List<PassFailStatsDto>>.Ok(result));
    }

    /// <summary>Danh sách top N sinh viên có GPA cao nhất.</summary>
    [HttpGet("top-students")]
    public async Task<ActionResult<ApiResponse<List<TopStudentDto>>>> GetTopStudents(
        [FromQuery] int topN = 10,
        [FromQuery] string? semester = null)
    {
        var result = await _statsService.GetTopStudentsAsync(topN, semester);
        return Ok(ApiResponse<List<TopStudentDto>>.Ok(result));
    }

    /// <summary>Danh sách sinh viên có cảnh báo học vụ (GPA dưới ngưỡng hoặc có môn rớt).</summary>
    [HttpGet("academic-warnings")]
    public async Task<ActionResult<ApiResponse<List<AcademicWarningDto>>>> GetAcademicWarnings(
        [FromQuery] decimal gpaThreshold = 5.0m)
    {
        var result = await _statsService.GetAcademicWarningsAsync(gpaThreshold);
        return Ok(ApiResponse<List<AcademicWarningDto>>.Ok(result));
    }
}
