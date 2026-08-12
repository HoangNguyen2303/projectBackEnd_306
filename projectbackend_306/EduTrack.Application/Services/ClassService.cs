using EduTrack.Application.Common;
using EduTrack.Application.DTOs.Classes;
using EduTrack.Application.Interfaces;
using EduTrack.Domain.Entities;
using FluentValidation;

namespace EduTrack.Application.Services;

public sealed class ClassService : IClassService
{
    private readonly IClassRepository _repository;
    private readonly IValidator<ClassQueryDto> _queryValidator;
    private readonly IValidator<CreateClassDto> _createValidator;
    private readonly IValidator<UpdateClassDto> _updateValidator;

    public ClassService(
        IClassRepository repository,
        IValidator<ClassQueryDto> queryValidator,
        IValidator<CreateClassDto> createValidator,
        IValidator<UpdateClassDto> updateValidator)
    {
        _repository = repository;
        _queryValidator = queryValidator;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<ApiResponse<PagedResult<ClassDto>>> GetAsync(
        ClassQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var validation = await _queryValidator.ValidateAsync(query, cancellationToken);
        if (!validation.IsValid)
            return ApiResponse<PagedResult<ClassDto>>.Fail(validation.ToErrorMessage());

        var (items, totalCount) = await _repository.GetPagedAsync(
            query.Search?.Trim(),
            query.CourseId,
            query.TeacherId,
            query.Semester?.Trim(),
            query.Page,
            query.PageSize,
            cancellationToken);

        return ApiResponse<PagedResult<ClassDto>>.Ok(new PagedResult<ClassDto>
        {
            Items = items.Select(Map).ToArray(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        });
    }

    public async Task<ApiResponse<ClassDto>> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var classSection = await _repository.GetByIdAsync(id, false, cancellationToken);
        return classSection is null
            ? ApiResponse<ClassDto>.Fail("Class not found.", 404)
            : ApiResponse<ClassDto>.Ok(Map(classSection));
    }

    public async Task<ApiResponse<ClassRosterDto>> GetRosterAsync(
        int id,
        string? currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var classSection = await _repository.GetWithRosterAsync(id, cancellationToken);
        if (classSection is null)
            return ApiResponse<ClassRosterDto>.Fail("Class not found.", 404);

        if (!isAdmin &&
            (string.IsNullOrWhiteSpace(currentUserId) ||
             !string.Equals(classSection.Teacher?.UserId, currentUserId, StringComparison.Ordinal)))
        {
            return ApiResponse<ClassRosterDto>.Fail(
                "Teachers may view only the roster of classes assigned to them.",
                403);
        }

        var students = classSection.Enrollments
            .OrderBy(enrollment => enrollment.Student!.StudentCode)
            .Select(enrollment => new ClassStudentDto
            {
                EnrollmentId = enrollment.Id,
                StudentId = enrollment.StudentId,
                StudentCode = enrollment.Student?.StudentCode ?? string.Empty,
                FullName = enrollment.Student?.FullName ?? string.Empty,
                Email = enrollment.Student?.Email ?? string.Empty,
                EnrolledAt = enrollment.EnrolledAt
            })
            .ToArray();

        return ApiResponse<ClassRosterDto>.Ok(new ClassRosterDto
        {
            ClassId = classSection.Id,
            ClassCode = classSection.ClassCode,
            MaxCapacity = classSection.MaxCapacity,
            EnrolledCount = students.Length,
            Students = students
        });
    }

    public async Task<ApiResponse<ClassDto>> CreateAsync(
        CreateClassDto dto,
        CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            return ApiResponse<ClassDto>.Fail(validation.ToErrorMessage());

        var classCode = NormalizeCode(dto.ClassCode);
        var referenceError = await ValidateReferencesAsync(
            classCode,
            null,
            dto.CourseId,
            dto.TeacherId,
            cancellationToken);
        if (referenceError is not null)
            return referenceError;

        var classSection = new ClassSection
        {
            ClassCode = classCode,
            CourseId = dto.CourseId,
            TeacherId = dto.TeacherId,
            Semester = dto.Semester.Trim(),
            MaxCapacity = dto.MaxCapacity
        };

        await _repository.AddAsync(classSection, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var created = await _repository.GetByIdAsync(classSection.Id, false, cancellationToken)
            ?? classSection;
        return ApiResponse<ClassDto>.Ok(Map(created), "Class created.", 201);
    }

    public async Task<ApiResponse<ClassDto>> UpdateAsync(
        int id,
        UpdateClassDto dto,
        CancellationToken cancellationToken = default)
    {
        var validation = await _updateValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            return ApiResponse<ClassDto>.Fail(validation.ToErrorMessage());

        var classSection = await _repository.GetByIdAsync(id, true, cancellationToken);
        if (classSection is null)
            return ApiResponse<ClassDto>.Fail("Class not found.", 404);

        if (dto.MaxCapacity < classSection.Enrollments.Count)
        {
            return ApiResponse<ClassDto>.Fail(
                "Max capacity cannot be lower than the current enrollment count.",
                409);
        }

        var classCode = NormalizeCode(dto.ClassCode);
        var referenceError = await ValidateReferencesAsync(
            classCode,
            id,
            dto.CourseId,
            dto.TeacherId,
            cancellationToken);
        if (referenceError is not null)
            return referenceError;

        classSection.ClassCode = classCode;
        classSection.CourseId = dto.CourseId;
        classSection.TeacherId = dto.TeacherId;
        classSection.Semester = dto.Semester.Trim();
        classSection.MaxCapacity = dto.MaxCapacity;

        await _repository.SaveChangesAsync(cancellationToken);
        var updated = await _repository.GetByIdAsync(id, false, cancellationToken) ?? classSection;
        return ApiResponse<ClassDto>.Ok(Map(updated), "Class updated.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var classSection = await _repository.GetByIdAsync(id, true, cancellationToken);
        if (classSection is null)
            return ApiResponse<bool>.Fail("Class not found.", 404);

        if (classSection.Enrollments.Count > 0)
            return ApiResponse<bool>.Fail("Cannot delete a class that has enrollments.", 409);

        _repository.Remove(classSection);
        await _repository.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.Ok(true, "Class deleted.", 204);
    }

    private async Task<ApiResponse<ClassDto>?> ValidateReferencesAsync(
        string classCode,
        int? excludingId,
        int courseId,
        int teacherId,
        CancellationToken cancellationToken)
    {
        if (await _repository.CodeExistsAsync(classCode, excludingId, cancellationToken))
            return ApiResponse<ClassDto>.Fail("Class code already exists.", 409);

        if (!await _repository.CourseExistsAsync(courseId, cancellationToken))
            return ApiResponse<ClassDto>.Fail("Course not found.", 400);

        if (!await _repository.TeacherExistsAsync(teacherId, cancellationToken))
            return ApiResponse<ClassDto>.Fail("Teacher not found.", 400);

        return null;
    }

    private static string NormalizeCode(string value) => value.Trim().ToUpperInvariant();

    private static ClassDto Map(ClassSection classSection) => new()
    {
        Id = classSection.Id,
        ClassCode = classSection.ClassCode,
        Semester = classSection.Semester,
        MaxCapacity = classSection.MaxCapacity,
        EnrolledCount = classSection.Enrollments.Count,
        CourseId = classSection.CourseId,
        CourseCode = classSection.Course?.CourseCode ?? string.Empty,
        CourseName = classSection.Course?.Name ?? string.Empty,
        TeacherId = classSection.TeacherId,
        TeacherCode = classSection.Teacher?.TeacherCode ?? string.Empty,
        TeacherName = classSection.Teacher?.FullName ?? string.Empty
    };
}
