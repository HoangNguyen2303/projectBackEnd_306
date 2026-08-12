using EduTrack.Application.Common;
using EduTrack.Application.DTOs.Auth;

namespace EduTrack.Application.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto dto);
    Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto dto);
}
