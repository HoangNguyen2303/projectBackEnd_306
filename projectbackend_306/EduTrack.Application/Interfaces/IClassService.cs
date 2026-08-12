using EduTrack.Application.Common;
using EduTrack.Application.DTOs.Classes;

namespace EduTrack.Application.Interfaces;

public interface IClassService
{
    Task<ApiResponse<PagedResult<ClassDto>>> GetAsync(
        ClassQueryDto query,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<ClassDto>> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<ClassRosterDto>> GetRosterAsync(
        int id,
        string? currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<ClassDto>> CreateAsync(
        CreateClassDto dto,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<ClassDto>> UpdateAsync(
        int id,
        UpdateClassDto dto,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}
