using EduTrack.Application.Common;
using EduTrack.Application.DTOs.Departments;

namespace EduTrack.Application.Interfaces;

public interface IDepartmentService
{
    Task<ApiResponse<PagedResult<DepartmentDto>>> GetAsync(
        DepartmentQueryDto query,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<DepartmentDto>> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<DepartmentDto>> CreateAsync(
        CreateDepartmentDto dto,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<DepartmentDto>> UpdateAsync(
        int id,
        UpdateDepartmentDto dto,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}
