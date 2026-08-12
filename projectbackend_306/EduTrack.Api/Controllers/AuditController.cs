using EduTrack.Application.Common;
using EduTrack.Application.DTOs;
using EduTrack.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.Api.Controllers;

[ApiController]
[Route("api/audit")]
[Authorize(Roles = "Admin")]
public class AuditController : ControllerBase
{
    private readonly IStatisticsService _statsService;

    public AuditController(IStatisticsService statsService)
    {
        _statsService = statsService;
    }

    /// <summary>Lịch sử thay đổi điểm (audit log) — chỉ Admin được xem.</summary>
    [HttpGet("grades")]
    public async Task<ActionResult<ApiResponse<List<AuditLogDto>>>> GetGradeAuditLogs(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var result = await _statsService.GetAuditLogsAsync(from, to);
        return Ok(ApiResponse<List<AuditLogDto>>.Ok(result));
    }
}
