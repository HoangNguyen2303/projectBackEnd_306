using EduTrack.Application.Common;
using EduTrack.Application.DTOs.Users;

namespace EduTrack.Application.Interfaces;

public interface IUserService
{
    // Self-service (dùng cho user thường)
    Task<ApiResponse<UserProfileDto>> GetProfileAsync(string userId);
    Task<ApiResponse<UserProfileDto>> UpdateProfileAsync(string userId, UpdateProfileDto dto);
    Task<ApiResponse<string>> ChangePasswordAsync(string userId, ChangePasswordDto dto);

    // Admin
    Task<ApiResponse<List<UserListItemDto>>> GetAllUsersAsync();
    Task<ApiResponse<UserProfileDto>> GetUserByIdAsync(string userId);
    Task<ApiResponse<UserProfileDto>> AdminUpdateUserAsync(string userId, AdminUpdateUserDto dto);
    Task<ApiResponse<string>> SetLockStatusAsync(string userId, bool isLocked);
}