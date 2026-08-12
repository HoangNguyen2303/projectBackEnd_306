using System.Security.Claims;
using EduTrack.Application.Common;
using EduTrack.Application.DTOs.Enrollments;
using EduTrack.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.Api.Controllers;

[ApiController]
[Authorize(Roles = "Student,Admin")]
[Route("api/[controller]")]
public sealed class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _service;

    public EnrollmentsController(IEnrollmentService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<EnrollmentDto>>>> Get(
        [FromQuery] EnrollmentQueryDto query,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetAsync(
            query,
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            User.IsInRole("Admin"),
            cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<EnrollmentDto>>> Enroll(
        CreateEnrollmentDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _service.EnrollAsync(
            dto,
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            User.IsInRole("Admin"),
            cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> Drop(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _service.DropAsync(
            id,
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            User.IsInRole("Admin"),
            cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
