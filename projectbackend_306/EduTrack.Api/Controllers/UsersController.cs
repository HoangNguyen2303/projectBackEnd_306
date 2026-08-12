using System.Security.Claims;
using EduTrack.Application.DTOs.Users;
using EduTrack.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    private string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException();

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var result = await _userService.GetProfileAsync(CurrentUserId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileDto dto)
    {
        var result = await _userService.UpdateProfileAsync(CurrentUserId, dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var result = await _userService.ChangePasswordAsync(CurrentUserId, dto);
        return StatusCode(result.StatusCode, result);
    }
}