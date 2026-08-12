using EduTrack.Application.DTOs.Users;
using EduTrack.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminUsersController : ControllerBase
{
    private readonly IUserService _userService;

    public AdminUsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var result = await _userService.GetAllUsersAsync();
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(string id)
    {
        var result = await _userService.GetUserByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] AdminUpdateUserDto dto)
    {
        var result = await _userService.AdminUpdateUserAsync(id, dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id}/lock")]
    public async Task<IActionResult> LockUser(string id)
    {
        var result = await _userService.SetLockStatusAsync(id, true);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id}/unlock")]
    public async Task<IActionResult> UnlockUser(string id)
    {
        var result = await _userService.SetLockStatusAsync(id, false);
        return StatusCode(result.StatusCode, result);
    }
}