using EduTrack.Application.Common;
using EduTrack.Application.DTOs.Courses;

namespace EduTrack.Application.Interfaces;

public interface ICourseService
{
    Task<ApiResponse<PagedResult<CourseDto>>> GetAsync(
        CourseQueryDto query,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<CourseDto>> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<CourseDto>> CreateAsync(
        CreateCourseDto dto,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<CourseDto>> UpdateAsync(
        int id,
        UpdateCourseDto dto,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}
