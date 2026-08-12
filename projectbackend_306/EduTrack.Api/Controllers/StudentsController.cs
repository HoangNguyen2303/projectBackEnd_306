using EduTrack.Application.Common;
using EduTrack.Application.DTOs.Auth.Student;
using EduTrack.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentsController(IStudentService studentService) => _studentService = studentService;

    [Authorize(Roles = "Admin,Teacher")]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<StudentDto>>>> Get(
        [FromQuery] StudentQueryDto query,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _studentService.GetPagedAsync(query, cancellationToken);
        var paged = new PagedResult<StudentDto>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
        return StatusCode(200, ApiResponse<PagedResult<StudentDto>>.Ok(paged));
    }

    [Authorize(Roles = "Admin,Teacher")]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<StudentDto>>> GetById(int id)
    {
        var student = await _studentService.GetByIdAsync(id);
        if (student is null)
            return StatusCode(404, ApiResponse<StudentDto>.Fail("Không tìm thấy sinh viên này.", 404));

        return StatusCode(200, ApiResponse<StudentDto>.Ok(student));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<StudentDto>>> Create([FromBody] CreateStudentDto dto)
    {
        try
        {
            var student = await _studentService.CreateAsync(dto);
            var response = ApiResponse<StudentDto>.Ok(student, "Tạo sinh viên thành công.", 201);
            return CreatedAtAction(nameof(GetById), new { id = student.Id }, response);
        }
        catch (Exception ex) when (ex is ArgumentException or KeyNotFoundException)
        {
            return StatusCode(400, ApiResponse<StudentDto>.Fail(ex.Message, 400));
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> Update(int id, [FromBody] CreateStudentDto dto)
    {
        var success = await _studentService.UpdateAsync(id, dto);
        if (!success)
            return StatusCode(404, ApiResponse<bool>.Fail("Không tìm thấy sinh viên để cập nhật.", 404));

        return StatusCode(200, ApiResponse<bool>.Ok(true, "Cập nhật thông tin sinh viên thành công."));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var success = await _studentService.DeleteAsync(id);
        if (!success)
            return StatusCode(404, ApiResponse<bool>.Fail("Không tìm thấy sinh viên để xóa.", 404));

        return StatusCode(200, ApiResponse<bool>.Ok(true, "Xóa sinh viên thành công."));
    }
}
