namespace EduTrack.Application.DTOs.Courses;

public sealed class CourseQueryDto
{
    public string? Search { get; set; }
    public int? DepartmentId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public sealed class CourseDto
{
    public int Id { get; init; }
    public string CourseCode { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int Credits { get; init; }
    public int DepartmentId { get; init; }
    public string DepartmentCode { get; init; } = string.Empty;
    public string DepartmentName { get; init; } = string.Empty;
}

public sealed class CreateCourseDto
{
    public string CourseCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Credits { get; set; }
    public int DepartmentId { get; set; }
}

public sealed class UpdateCourseDto
{
    public string CourseCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Credits { get; set; }
    public int DepartmentId { get; set; }
}
