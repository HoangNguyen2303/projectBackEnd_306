namespace EduTrack.Application.DTOs;

public class TopStudentDto
{
    public int StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public decimal Gpa { get; set; }
    public string Classification { get; set; } = string.Empty;
}
