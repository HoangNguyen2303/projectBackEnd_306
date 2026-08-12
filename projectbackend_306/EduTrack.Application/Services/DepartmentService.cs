using EduTrack.Application.Common;
using EduTrack.Application.DTOs.Departments;
using EduTrack.Application.Interfaces;
using EduTrack.Domain.Entities;
using FluentValidation;

namespace EduTrack.Application.Services;

public sealed class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _repository;
    private readonly IValidator<DepartmentQueryDto> _queryValidator;
    private readonly IValidator<CreateDepartmentDto> _createValidator;
    private readonly IValidator<UpdateDepartmentDto> _updateValidator;

    public DepartmentService(
        IDepartmentRepository repository,
        IValidator<DepartmentQueryDto> queryValidator,
        IValidator<CreateDepartmentDto> createValidator,
        IValidator<UpdateDepartmentDto> updateValidator)
    {
        _repository = repository;
        _queryValidator = queryValidator;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<ApiResponse<PagedResult<DepartmentDto>>> GetAsync(
        DepartmentQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var validation = await _queryValidator.ValidateAsync(query, cancellationToken);
        if (!validation.IsValid)
            return ApiResponse<PagedResult<DepartmentDto>>.Fail(validation.ToErrorMessage());

        var (items, totalCount) = await _repository.GetPagedAsync(
            query.Search?.Trim(),
            query.Page,
            query.PageSize,
            cancellationToken);

        return ApiResponse<PagedResult<DepartmentDto>>.Ok(new PagedResult<DepartmentDto>
        {
            Items = items.Select(Map).ToArray(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        });
    }

    public async Task<ApiResponse<DepartmentDto>> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var department = await _repository.GetByIdAsync(id, false, cancellationToken);
        return department is null
            ? ApiResponse<DepartmentDto>.Fail("Department not found.", 404)
            : ApiResponse<DepartmentDto>.Ok(Map(department));
    }

    public async Task<ApiResponse<DepartmentDto>> CreateAsync(
        CreateDepartmentDto dto,
        CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            return ApiResponse<DepartmentDto>.Fail(validation.ToErrorMessage());

        var code = NormalizeCode(dto.Code);
        if (await _repository.CodeExistsAsync(code, null, cancellationToken))
            return ApiResponse<DepartmentDto>.Fail("Department code already exists.", 409);

        var department = new Department
        {
            Code = code,
            Name = dto.Name.Trim()
        };

        await _repository.AddAsync(department, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var created = await _repository.GetByIdAsync(department.Id, false, cancellationToken)
            ?? department;
        return ApiResponse<DepartmentDto>.Ok(Map(created), "Department created.", 201);
    }

    public async Task<ApiResponse<DepartmentDto>> UpdateAsync(
        int id,
        UpdateDepartmentDto dto,
        CancellationToken cancellationToken = default)
    {
        var validation = await _updateValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            return ApiResponse<DepartmentDto>.Fail(validation.ToErrorMessage());

        var department = await _repository.GetByIdAsync(id, true, cancellationToken);
        if (department is null)
            return ApiResponse<DepartmentDto>.Fail("Department not found.", 404);

        var code = NormalizeCode(dto.Code);
        if (await _repository.CodeExistsAsync(code, id, cancellationToken))
            return ApiResponse<DepartmentDto>.Fail("Department code already exists.", 409);

        department.Code = code;
        department.Name = dto.Name.Trim();

        await _repository.SaveChangesAsync(cancellationToken);
        var updated = await _repository.GetByIdAsync(id, false, cancellationToken) ?? department;
        return ApiResponse<DepartmentDto>.Ok(Map(updated), "Department updated.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var department = await _repository.GetByIdAsync(id, true, cancellationToken);
        if (department is null)
            return ApiResponse<bool>.Fail("Department not found.", 404);

        if (await _repository.HasRelatedDataAsync(id, cancellationToken))
        {
            return ApiResponse<bool>.Fail(
                "Cannot delete a department that has students, teachers, or courses.",
                409);
        }

        _repository.Remove(department);
        await _repository.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.Ok(true, "Department deleted.", 204);
    }

    private static string NormalizeCode(string value) => value.Trim().ToUpperInvariant();

    private static DepartmentDto Map(Department department) => new()
    {
        Id = department.Id,
        Code = department.Code,
        Name = department.Name,
        StudentCount = department.Students.Count,
        TeacherCount = department.Teachers.Count,
        CourseCount = department.Courses.Count
    };
}
