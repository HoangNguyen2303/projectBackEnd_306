using EduTrack.Application.Common;
using EduTrack.Application.DTOs.Departments;
using EduTrack.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _service;

    public DepartmentsController(IDepartmentService service) => _service = service;

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<DepartmentDto>>>> Get(
        [FromQuery] DepartmentQueryDto query,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetAsync(query, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<DepartmentDto>>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<DepartmentDto>>> Create(
        CreateDepartmentDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);
        if (result.Success && result.Data is not null)
            return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result);

        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<DepartmentDto>>> Update(
        int id,
        UpdateDepartmentDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(id, dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _service.DeleteAsync(id, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
