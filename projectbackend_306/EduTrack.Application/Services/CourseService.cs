using EduTrack.Application.Common;
using EduTrack.Application.DTOs.Courses;
using EduTrack.Application.Interfaces;
using EduTrack.Domain.Entities;
using FluentValidation;

namespace EduTrack.Application.Services;

public sealed class CourseService : ICourseService
{
    private readonly ICourseRepository _repository;
    private readonly IValidator<CourseQueryDto> _queryValidator;
    private readonly IValidator<CreateCourseDto> _createValidator;
    private readonly IValidator<UpdateCourseDto> _updateValidator;

    public CourseService(
        ICourseRepository repository,
        IValidator<CourseQueryDto> queryValidator,
        IValidator<CreateCourseDto> createValidator,
        IValidator<UpdateCourseDto> updateValidator)
    {
        _repository = repository;
        _queryValidator = queryValidator;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<ApiResponse<PagedResult<CourseDto>>> GetAsync(
        CourseQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var validation = await _queryValidator.ValidateAsync(query, cancellationToken);
        if (!validation.IsValid)
            return ApiResponse<PagedResult<CourseDto>>.Fail(validation.ToErrorMessage());

        var (items, totalCount) = await _repository.GetPagedAsync(
            query.Search?.Trim(),
            query.DepartmentId,
            query.Page,
            query.PageSize,
            cancellationToken);

        return ApiResponse<PagedResult<CourseDto>>.Ok(new PagedResult<CourseDto>
        {
            Items = items.Select(Map).ToArray(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        });
    }

    public async Task<ApiResponse<CourseDto>> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var course = await _repository.GetByIdAsync(id, false, cancellationToken);
        return course is null
            ? ApiResponse<CourseDto>.Fail("Course not found.", 404)
            : ApiResponse<CourseDto>.Ok(Map(course));
    }

    public async Task<ApiResponse<CourseDto>> CreateAsync(
        CreateCourseDto dto,
        CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            return ApiResponse<CourseDto>.Fail(validation.ToErrorMessage());

        var courseCode = NormalizeCode(dto.CourseCode);
        if (await _repository.CodeExistsAsync(courseCode, null, cancellationToken))
            return ApiResponse<CourseDto>.Fail("Course code already exists.", 409);

        if (!await _repository.DepartmentExistsAsync(dto.DepartmentId, cancellationToken))
            return ApiResponse<CourseDto>.Fail("Department not found.", 400);

        var course = new Course
        {
            CourseCode = courseCode,
            Name = dto.Name.Trim(),
            Credits = dto.Credits,
            DepartmentId = dto.DepartmentId
        };

        await _repository.AddAsync(course, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var created = await _repository.GetByIdAsync(course.Id, false, cancellationToken) ?? course;
        return ApiResponse<CourseDto>.Ok(Map(created), "Course created.", 201);
    }

    public async Task<ApiResponse<CourseDto>> UpdateAsync(
        int id,
        UpdateCourseDto dto,
        CancellationToken cancellationToken = default)
    {
        var validation = await _updateValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            return ApiResponse<CourseDto>.Fail(validation.ToErrorMessage());

        var course = await _repository.GetByIdAsync(id, true, cancellationToken);
        if (course is null)
            return ApiResponse<CourseDto>.Fail("Course not found.", 404);

        var courseCode = NormalizeCode(dto.CourseCode);
        if (await _repository.CodeExistsAsync(courseCode, id, cancellationToken))
            return ApiResponse<CourseDto>.Fail("Course code already exists.", 409);

        if (!await _repository.DepartmentExistsAsync(dto.DepartmentId, cancellationToken))
            return ApiResponse<CourseDto>.Fail("Department not found.", 400);

        course.CourseCode = courseCode;
        course.Name = dto.Name.Trim();
        course.Credits = dto.Credits;
        course.DepartmentId = dto.DepartmentId;

        await _repository.SaveChangesAsync(cancellationToken);
        var updated = await _repository.GetByIdAsync(id, false, cancellationToken) ?? course;
        return ApiResponse<CourseDto>.Ok(Map(updated), "Course updated.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var course = await _repository.GetByIdAsync(id, true, cancellationToken);
        if (course is null)
            return ApiResponse<bool>.Fail("Course not found.", 404);

        if (await _repository.HasClassesAsync(id, cancellationToken))
            return ApiResponse<bool>.Fail("Cannot delete a course that has class sections.", 409);

        _repository.Remove(course);
        await _repository.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.Ok(true, "Course deleted.", 204);
    }

    private static string NormalizeCode(string value) => value.Trim().ToUpperInvariant();

    private static CourseDto Map(Course course) => new()
    {
        Id = course.Id,
        CourseCode = course.CourseCode,
        Name = course.Name,
        Credits = course.Credits,
        DepartmentId = course.DepartmentId,
        DepartmentCode = course.Department?.Code ?? string.Empty,
        DepartmentName = course.Department?.Name ?? string.Empty
    };
}
