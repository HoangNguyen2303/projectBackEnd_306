namespace EduTrack.Application.DTOs.Departments;

public sealed class DepartmentQueryDto
{
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public sealed class DepartmentDto
{
    public int Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int StudentCount { get; init; }
    public int TeacherCount { get; init; }
    public int CourseCount { get; init; }
}

public sealed class CreateDepartmentDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public sealed class UpdateDepartmentDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
