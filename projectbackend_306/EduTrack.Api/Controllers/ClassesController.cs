using System.Security.Claims;
using EduTrack.Application.Common;
using EduTrack.Application.DTOs.Classes;
using EduTrack.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ClassesController : ControllerBase
{
    private readonly IClassService _service;

    public ClassesController(IClassService service) => _service = service;

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ClassDto>>>> Get(
        [FromQuery] ClassQueryDto query,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetAsync(query, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ClassDto>>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = "Admin,Teacher")]
    [HttpGet("{id:int}/students")]
    public async Task<ActionResult<ApiResponse<ClassRosterDto>>> GetStudents(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetRosterAsync(
            id,
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            User.IsInRole("Admin"),
            cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ClassDto>>> Create(
        CreateClassDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);
        if (result.Success && result.Data is not null)
            return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result);

        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<ClassDto>>> Update(
        int id,
        UpdateClassDto dto,
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
