using EduTrack.Application.Common;
using EduTrack.Application.DTOs.Enrollments;
using EduTrack.Application.Interfaces;
using EduTrack.Domain.Entities;
using FluentValidation;

namespace EduTrack.Application.Services;

public sealed class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _repository;
    private readonly IValidator<EnrollmentQueryDto> _queryValidator;
    private readonly IValidator<CreateEnrollmentDto> _createValidator;

    public EnrollmentService(
        IEnrollmentRepository repository,
        IValidator<EnrollmentQueryDto> queryValidator,
        IValidator<CreateEnrollmentDto> createValidator)
    {
        _repository = repository;
        _queryValidator = queryValidator;
        _createValidator = createValidator;
    }

    public async Task<ApiResponse<PagedResult<EnrollmentDto>>> GetAsync(
        EnrollmentQueryDto query,
        string? currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var validation = await _queryValidator.ValidateAsync(query, cancellationToken);
        if (!validation.IsValid)
            return ApiResponse<PagedResult<EnrollmentDto>>.Fail(validation.ToErrorMessage());

        var studentId = query.StudentId;
        if (!isAdmin)
        {
            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return ApiResponse<PagedResult<EnrollmentDto>>.Fail(
                    "Authenticated user identifier is missing.",
                    403);
            }

            var currentStudent = await _repository.GetStudentByUserIdAsync(
                currentUserId,
                cancellationToken);
            if (currentStudent is null)
                return ApiResponse<PagedResult<EnrollmentDto>>.Fail("Student profile not found.", 404);

            if (studentId.HasValue && studentId.Value != currentStudent.Id)
            {
                return ApiResponse<PagedResult<EnrollmentDto>>.Fail(
                    "Students may view only their own enrollments.",
                    403);
            }

            studentId = currentStudent.Id;
        }
        else if (studentId.HasValue &&
                 await _repository.GetStudentAsync(studentId.Value, cancellationToken) is null)
        {
            return ApiResponse<PagedResult<EnrollmentDto>>.Fail("Student not found.", 404);
        }

        var (items, totalCount) = await _repository.GetPagedAsync(
            studentId,
            query.ClassId,
            query.Semester?.Trim(),
            query.Page,
            query.PageSize,
            cancellationToken);

        return ApiResponse<PagedResult<EnrollmentDto>>.Ok(new PagedResult<EnrollmentDto>
        {
            Items = items.Select(Map).ToArray(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        });
    }

    public async Task<ApiResponse<EnrollmentDto>> EnrollAsync(
        CreateEnrollmentDto dto,
        string? currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            return ApiResponse<EnrollmentDto>.Fail(validation.ToErrorMessage());

        var student = await _repository.GetStudentAsync(dto.StudentId, cancellationToken);
        if (student is null)
            return ApiResponse<EnrollmentDto>.Fail("Student not found.", 404);

        if (!CanManageStudent(student, currentUserId, isAdmin))
            return ApiResponse<EnrollmentDto>.Fail("Students may manage only their own enrollments.", 403);

        var classSection = await _repository.GetClassAsync(dto.ClassId, cancellationToken);
        if (classSection is null)
            return ApiResponse<EnrollmentDto>.Fail("Class not found.", 404);

        if (await _repository.FindAsync(dto.StudentId, dto.ClassId, cancellationToken) is not null)
            return ApiResponse<EnrollmentDto>.Fail("Student is already enrolled in this class.", 409);

        if (classSection.Enrollments.Count >= classSection.MaxCapacity)
            return ApiResponse<EnrollmentDto>.Fail("Class is full.", 409);

        var enrollment = new Enrollment
        {
            StudentId = student.Id,
            Student = student,
            ClassId = classSection.Id,
            Class = classSection,
            EnrolledAt = DateTime.UtcNow
        };

        await _repository.AddAsync(enrollment, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return ApiResponse<EnrollmentDto>.Ok(Map(enrollment), "Enrollment created.", 201);
    }

    public async Task<ApiResponse<bool>> DropAsync(
        int enrollmentId,
        string? currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var enrollment = await _repository.GetByIdAsync(enrollmentId, cancellationToken);
        if (enrollment is null)
            return ApiResponse<bool>.Fail("Enrollment not found.", 404);

        if (enrollment.Student is null ||
            !CanManageStudent(enrollment.Student, currentUserId, isAdmin))
        {
            return ApiResponse<bool>.Fail("Students may manage only their own enrollments.", 403);
        }

        _repository.Remove(enrollment);
        await _repository.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.Ok(true, "Enrollment dropped.", 204);
    }

    private static bool CanManageStudent(Student student, string? currentUserId, bool isAdmin)
        => isAdmin ||
           (!string.IsNullOrWhiteSpace(currentUserId) &&
            string.Equals(student.UserId, currentUserId, StringComparison.Ordinal));

    private static EnrollmentDto Map(Enrollment enrollment) => new()
    {
        Id = enrollment.Id,
        StudentId = enrollment.StudentId,
        StudentCode = enrollment.Student?.StudentCode ?? string.Empty,
        StudentName = enrollment.Student?.FullName ?? string.Empty,
        ClassId = enrollment.ClassId,
        ClassCode = enrollment.Class?.ClassCode ?? string.Empty,
        CourseCode = enrollment.Class?.Course?.CourseCode ?? string.Empty,
        CourseName = enrollment.Class?.Course?.Name ?? string.Empty,
        Semester = enrollment.Class?.Semester ?? string.Empty,
        EnrolledAt = enrollment.EnrolledAt
    };
}
