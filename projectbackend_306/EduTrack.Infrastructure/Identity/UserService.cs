using EduTrack.Application.Common;
using EduTrack.Application.DTOs.Users;
using EduTrack.Application.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace EduTrack.Infrastructure.Identity;

public class UserService : IUserService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IEnrollmentRepository _enrollmentRepository;

    public UserService(UserManager<AppUser> userManager, IEnrollmentRepository enrollmentRepository)
    {
        _userManager = userManager;
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<ApiResponse<UserProfileDto>> GetProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return ApiResponse<UserProfileDto>.Fail("Không tìm thấy người dùng.", 404);

        var dto = MapToProfileDto(user);
        var student = await _enrollmentRepository.GetStudentByUserIdAsync(userId, CancellationToken.None);
        dto.StudentId = student?.Id;
        return ApiResponse<UserProfileDto>.Ok(dto);
    }

    public async Task<ApiResponse<UserProfileDto>> UpdateProfileAsync(string userId, UpdateProfileDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return ApiResponse<UserProfileDto>.Fail("Không tìm thấy người dùng.", 404);

        user.FullName = dto.FullName;
        user.PhoneNumber = dto.PhoneNumber;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(" ", result.Errors.Select(e => e.Description));
            return ApiResponse<UserProfileDto>.Fail(errors, 400);
        }

        return ApiResponse<UserProfileDto>.Ok(MapToProfileDto(user));
    }

    public async Task<ApiResponse<string>> ChangePasswordAsync(string userId, ChangePasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return ApiResponse<string>.Fail("Không tìm thấy người dùng.", 404);

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(" ", result.Errors.Select(e => e.Description));
            return ApiResponse<string>.Fail(errors, 400);
        }

        return ApiResponse<string>.Ok("Đổi mật khẩu thành công.");
    }

    public async Task<ApiResponse<List<UserListItemDto>>> GetAllUsersAsync()
    {
        var users = _userManager.Users
            .Select(u => new UserListItemDto
            {
                Id = u.Id,
                Email = u.Email ?? string.Empty,
                FullName = u.FullName,
                Role = u.Role.ToString(),
                IsLocked = u.IsLocked
            })
            .ToList();

        return ApiResponse<List<UserListItemDto>>.Ok(users);
    }

    public async Task<ApiResponse<UserProfileDto>> GetUserByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return ApiResponse<UserProfileDto>.Fail("Không tìm thấy người dùng.", 404);

        return ApiResponse<UserProfileDto>.Ok(MapToProfileDto(user));
    }

    public async Task<ApiResponse<UserProfileDto>> AdminUpdateUserAsync(string userId, AdminUpdateUserDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return ApiResponse<UserProfileDto>.Fail("Không tìm thấy người dùng.", 404);

        user.FullName = dto.FullName;
        user.PhoneNumber = dto.PhoneNumber;
        user.Role = dto.Role;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(" ", result.Errors.Select(e => e.Description));
            return ApiResponse<UserProfileDto>.Fail(errors, 400);
        }

        return ApiResponse<UserProfileDto>.Ok(MapToProfileDto(user));
    }

    public async Task<ApiResponse<string>> SetLockStatusAsync(string userId, bool isLocked)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return ApiResponse<string>.Fail("Không tìm thấy người dùng.", 404);

        user.IsLocked = isLocked;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(" ", result.Errors.Select(e => e.Description));
            return ApiResponse<string>.Fail(errors, 400);
        }

        return ApiResponse<string>.Ok(isLocked ? "Đã khoá tài khoản." : "Đã mở khoá tài khoản.");
    }

    private static UserProfileDto MapToProfileDto(AppUser user) => new()
    {
        Id = user.Id,
        Email = user.Email ?? string.Empty,
        FullName = user.FullName,
        PhoneNumber = user.PhoneNumber,
        Role = user.Role.ToString(),
        IsLocked = user.IsLocked
    };
}