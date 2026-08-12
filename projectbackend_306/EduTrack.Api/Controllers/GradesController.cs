using System.Security.Claims;
using EduTrack.Application.Common;
using EduTrack.Application.DTOs;
using EduTrack.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")] // -> api/grades, giống HealthController -> api/health
[Authorize]
public class GradesController : ControllerBase
{
    private readonly IGradeService _gradeService;

    public GradesController(IGradeService gradeService)
    {
        _gradeService = gradeService;
    }

    /// <summary>
    /// GET api/grades
    /// - Admin/Teacher: xem toàn bộ điểm.
    /// - Student: chỉ xem điểm của chính mình (tự lọc theo token, không nhận studentId từ query).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<GradeDto>>>> GetAll()
    {
        if (User.IsInRole("Admin") || User.IsInRole("Teacher"))
        {
            var all = await _gradeService.GetAllAsync();
            return Ok(ApiResponse<List<GradeDto>>.Ok(all));
        }

        var userId = GetCurrentUserId();
        var own = await _gradeService.GetByUserIdAsync(userId);
        return Ok(ApiResponse<List<GradeDto>>.Ok(own));
    }

    /// <summary>GET api/grades/{id}</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<GradeDto>>> GetById(int id)
    {
        try
        {
            var isPrivileged = User.IsInRole("Admin") || User.IsInRole("Teacher");
            var userId = GetCurrentUserId();
            var grade = await _gradeService.GetByIdAsync(id, userId, isPrivileged);
            return Ok(ApiResponse<GradeDto>.Ok(grade));
        }
        catch (NotFoundException ex)
        {
            return NotFound(ApiResponse<GradeDto>.Fail(ex.Message, StatusCodes.Status404NotFound));
        }
        catch (ForbiddenException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<GradeDto>.Fail(ex.Message, StatusCodes.Status403Forbidden));
        }
    }

    /// <summary>POST api/grades — chỉ giáo viên phụ trách lớp đó (hoặc Admin) được nhập điểm (§10.3).</summary>
    [HttpPost]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult<ApiResponse<GradeDto>>> Create([FromBody] CreateGradeDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<GradeDto>.Fail("Dữ liệu không hợp lệ"));

        try
        {
            var userId = GetCurrentUserId();
            var created = await _gradeService.CreateAsync(dto, userId, isAdmin: User.IsInRole("Admin"));
            return CreatedAtAction(nameof(GetById), new { id = created.Id },
                ApiResponse<GradeDto>.Ok(created, "Tạo điểm thành công", StatusCodes.Status201Created));
        }
        catch (NotFoundException ex)
        {
            return NotFound(ApiResponse<GradeDto>.Fail(ex.Message, StatusCodes.Status404NotFound));
        }
        catch (ForbiddenException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<GradeDto>.Fail(ex.Message, StatusCodes.Status403Forbidden));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<GradeDto>.Fail(ex.Message, StatusCodes.Status409Conflict));
        }
    }

    /// <summary>PUT api/grades/{id} — chỉ giáo viên phụ trách lớp đó (hoặc Admin) được sửa điểm (§10.3).</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult<ApiResponse<GradeDto>>> Update(int id, [FromBody] UpdateGradeDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<GradeDto>.Fail("Dữ liệu không hợp lệ"));

        try
        {
            var userId = GetCurrentUserId();
            var updated = await _gradeService.UpdateAsync(id, dto, userId, isAdmin: User.IsInRole("Admin"));
            return Ok(ApiResponse<GradeDto>.Ok(updated, "Cập nhật điểm thành công"));
        }
        catch (NotFoundException ex)
        {
            return NotFound(ApiResponse<GradeDto>.Fail(ex.Message, StatusCodes.Status404NotFound));
        }
        catch (ForbiddenException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<GradeDto>.Fail(ex.Message, StatusCodes.Status403Forbidden));
        }
    }

    /// <summary>DELETE api/grades/{id} — chỉ giáo viên phụ trách lớp đó (hoặc Admin) được xoá điểm (§10.3).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _gradeService.DeleteAsync(id, userId, isAdmin: User.IsInRole("Admin"));
            return Ok(ApiResponse<string>.Ok("Deleted", "Xoá điểm thành công"));
        }
        catch (NotFoundException ex)
        {
            return NotFound(ApiResponse<string>.Fail(ex.Message, StatusCodes.Status404NotFound));
        }
        catch (ForbiddenException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<string>.Fail(ex.Message, StatusCodes.Status403Forbidden));
        }
    }

    /// <summary>
    /// GET /api/students/{id}/grades — bảng điểm của 1 sinh viên (§9.4).
    /// Admin/Teacher xem ai cũng được; Student chỉ xem của chính mình.
    /// </summary>
    [HttpGet("/api/students/{id:int}/grades")]
    public async Task<ActionResult<ApiResponse<List<GradeDto>>>> GetGradesByStudent(int id)
    {
        try
        {
            var isPrivileged = User.IsInRole("Admin") || User.IsInRole("Teacher");
            var userId = GetCurrentUserId();
            var grades = await _gradeService.GetGradesByStudentAsync(id, userId, isPrivileged);
            return Ok(ApiResponse<List<GradeDto>>.Ok(grades));
        }
        catch (NotFoundException ex)
        {
            return NotFound(ApiResponse<List<GradeDto>>.Fail(ex.Message, StatusCodes.Status404NotFound));
        }
        catch (ForbiddenException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<List<GradeDto>>.Fail(ex.Message, StatusCodes.Status403Forbidden));
        }
    }

    /// <summary>
    /// GET /api/students/{id}/gpa — GPA + classification của 1 sinh viên (§9.4, §10.3).
    /// Admin/Teacher xem ai cũng được; Student chỉ xem của chính mình.
    /// </summary>
    [HttpGet("/api/students/{id:int}/gpa")]
    public async Task<ActionResult<ApiResponse<GpaDto>>> GetGpaByStudent(int id)
    {
        try
        {
            var isPrivileged = User.IsInRole("Admin") || User.IsInRole("Teacher");
            var userId = GetCurrentUserId();
            var gpa = await _gradeService.GetGpaByStudentAsync(id, userId, isPrivileged);
            return Ok(ApiResponse<GpaDto>.Ok(gpa));
        }
        catch (NotFoundException ex)
        {
            return NotFound(ApiResponse<GpaDto>.Fail(ex.Message, StatusCodes.Status404NotFound));
        }
        catch (ForbiddenException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<GpaDto>.Fail(ex.Message, StatusCodes.Status403Forbidden));
        }
    }

    // ----------------- helpers -----------------

    /// <summary>
    /// Lấy UserId từ token. Dùng ClaimTypes.NameIdentifier (fallback "sub") —
    /// chỉnh lại claim type nếu AuthController của nhóm cấu hình JWT khác.
    /// </summary>
    private string GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? User.FindFirstValue("sub")
               ?? string.Empty;
    }
}
