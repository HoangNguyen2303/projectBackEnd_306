using EduTrack.Application.Common;
using EduTrack.Application.DTOs.Auth;
using EduTrack.Application.Interfaces;
using EduTrack.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace EduTrack.Infrastructure.Identity;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;

    public AuthService(UserManager<AppUser> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto dto)
    {
        var existing = await _userManager.FindByEmailAsync(dto.Email);
        if (existing is not null)
            return ApiResponse<AuthResponseDto>.Fail("Email đã được sử dụng.", 409);

        var user = new AppUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName,
            Role = UserRole.Student
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(" ", result.Errors.Select(e => e.Description));
            return ApiResponse<AuthResponseDto>.Fail(errors, 400);
        }

        return BuildAuthResponse(user);
    }

    public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null || user.IsLocked)
            return ApiResponse<AuthResponseDto>.Fail("Email hoặc mật khẩu không đúng.", 401);

        var valid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!valid)
            return ApiResponse<AuthResponseDto>.Fail("Email hoặc mật khẩu không đúng.", 401);

        return BuildAuthResponse(user);
    }

    private ApiResponse<AuthResponseDto> BuildAuthResponse(AppUser user)
    {
        var (token, expiresAt) = _tokenService.GenerateToken(
            user.Id, user.Email!, user.FullName, user.Role.ToString());

        return ApiResponse<AuthResponseDto>.Ok(new AuthResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            Email = user.Email!,
            FullName = user.FullName,
            Role = user.Role.ToString()
        });
    }
}
