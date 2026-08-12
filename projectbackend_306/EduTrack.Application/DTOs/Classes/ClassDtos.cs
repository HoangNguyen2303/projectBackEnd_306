namespace EduTrack.Application.DTOs.Classes;

public sealed class ClassQueryDto
{
    public string? Search { get; set; }
    public int? CourseId { get; set; }
    public int? TeacherId { get; set; }
    public string? Semester { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public sealed class ClassDto
{
    public int Id { get; init; }
    public string ClassCode { get; init; } = string.Empty;
    public string Semester { get; init; } = string.Empty;
    public int MaxCapacity { get; init; }
    public int EnrolledCount { get; init; }
    public int AvailableSeats => Math.Max(0, MaxCapacity - EnrolledCount);
    public bool IsFull => EnrolledCount >= MaxCapacity;
    public int CourseId { get; init; }
    public string CourseCode { get; init; } = string.Empty;
    public string CourseName { get; init; } = string.Empty;
    public int TeacherId { get; init; }
    public string TeacherCode { get; init; } = string.Empty;
    public string TeacherName { get; init; } = string.Empty;
}

public sealed class CreateClassDto
{
    public string ClassCode { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public int TeacherId { get; set; }
    public string Semester { get; set; } = string.Empty;
    public int MaxCapacity { get; set; }
}

public sealed class UpdateClassDto
{
    public string ClassCode { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public int TeacherId { get; set; }
    public string Semester { get; set; } = string.Empty;
    public int MaxCapacity { get; set; }
}

public sealed class ClassStudentDto
{
    public int EnrollmentId { get; init; }
    public int StudentId { get; init; }
    public string StudentCode { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime EnrolledAt { get; init; }
}

public sealed class ClassRosterDto
{
    public int ClassId { get; init; }
    public string ClassCode { get; init; } = string.Empty;
    public int MaxCapacity { get; init; }
    public int EnrolledCount { get; init; }
    public IReadOnlyList<ClassStudentDto> Students { get; init; } = Array.Empty<ClassStudentDto>();
}
