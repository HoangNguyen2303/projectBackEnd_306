namespace EduTrack.Application.DTOs;

public class StudentsByDepartmentDto
{
    public int DepartmentId { get; set; }
    public string DepartmentCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public int StudentCount { get; set; }
}
