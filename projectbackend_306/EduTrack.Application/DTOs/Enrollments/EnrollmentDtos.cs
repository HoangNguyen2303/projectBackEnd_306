namespace EduTrack.Application.DTOs.Enrollments;

public sealed class EnrollmentQueryDto
{
    public int? StudentId { get; set; }
    public int? ClassId { get; set; }
    public string? Semester { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public sealed class CreateEnrollmentDto
{
    public int StudentId { get; set; }
    public int ClassId { get; set; }
}

public sealed class EnrollmentDto
{
    public int Id { get; init; }
    public int StudentId { get; init; }
    public string StudentCode { get; init; } = string.Empty;
    public string StudentName { get; init; } = string.Empty;
    public int ClassId { get; init; }
    public string ClassCode { get; init; } = string.Empty;
    public string CourseCode { get; init; } = string.Empty;
    public string CourseName { get; init; } = string.Empty;
    public string Semester { get; init; } = string.Empty;
    public DateTime EnrolledAt { get; init; }
}
