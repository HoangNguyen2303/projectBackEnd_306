using EduTrack.Application.Common;
using EduTrack.Application.DTOs.Auth.Teacher;
using EduTrack.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TeachersController : ControllerBase
{
    private readonly ITeacherService _teacherService;

    public TeachersController(ITeacherService teacherService) => _teacherService = teacherService;

    [Authorize(Roles = "Admin,Teacher")]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<TeacherDto>>>> Get(
        [FromQuery] TeacherQueryDto query,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _teacherService.GetPagedAsync(query, cancellationToken);
        var paged = new PagedResult<TeacherDto>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
        return StatusCode(200, ApiResponse<PagedResult<TeacherDto>>.Ok(paged));
    }

    [Authorize(Roles = "Admin,Teacher")]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<TeacherDto>>> GetById(int id)
    {
        var teacher = await _teacherService.GetByIdAsync(id);
        if (teacher is null)
            return StatusCode(404, ApiResponse<TeacherDto>.Fail("Không tìm thấy giảng viên này.", 404));

        return StatusCode(200, ApiResponse<TeacherDto>.Ok(teacher));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<TeacherDto>>> Create([FromBody] CreateTeacherDto dto)
    {
        try
        {
            var teacher = await _teacherService.CreateAsync(dto);
            var response = ApiResponse<TeacherDto>.Ok(teacher, "Tạo giảng viên thành công.", 201);
            return CreatedAtAction(nameof(GetById), new { id = teacher.Id }, response);
        }
        catch (Exception ex) when (ex is ArgumentException or KeyNotFoundException)
        {
            return StatusCode(400, ApiResponse<TeacherDto>.Fail(ex.Message, 400));
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> Update(int id, [FromBody] CreateTeacherDto dto)
    {
        var success = await _teacherService.UpdateAsync(id, dto);
        if (!success)
            return StatusCode(404, ApiResponse<bool>.Fail("Không tìm thấy giảng viên để cập nhật.", 404));

        return StatusCode(200, ApiResponse<bool>.Ok(true, "Cập nhật thông tin giảng viên thành công."));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var success = await _teacherService.DeleteAsync(id);
        if (!success)
            return StatusCode(404, ApiResponse<bool>.Fail("Không tìm thấy giảng viên để xóa.", 404));

        return StatusCode(200, ApiResponse<bool>.Ok(true, "Xóa giảng viên thành công."));
    }
}
