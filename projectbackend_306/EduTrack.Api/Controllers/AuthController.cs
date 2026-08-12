using EduTrack.Application.Common;
using EduTrack.Application.DTOs.Auth;
using EduTrack.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register(RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login(LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        return StatusCode(result.StatusCode, result);
    }
}
