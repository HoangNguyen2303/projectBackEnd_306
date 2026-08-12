using EduTrack.Application.Common;
using EduTrack.Application.DTOs.Enrollments;

namespace EduTrack.Application.Interfaces;

public interface IEnrollmentService
{
    Task<ApiResponse<PagedResult<EnrollmentDto>>> GetAsync(
        EnrollmentQueryDto query,
        string? currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<EnrollmentDto>> EnrollAsync(
        CreateEnrollmentDto dto,
        string? currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> DropAsync(
        int enrollmentId,
        string? currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default);
}
