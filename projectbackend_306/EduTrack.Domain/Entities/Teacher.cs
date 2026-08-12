using EduTrack.Domain.Common;

namespace EduTrack.Domain.Entities;

public class Teacher : BaseEntity
{
    public string TeacherCode { get; set; } = string.Empty;  // unique
    public string FullName { get; set; } = string.Empty;
    public string? Specialization { get; set; }
    public string Email { get; set; } = string.Empty;        // unique

    public int DepartmentId { get; set; }
    public Department? Department { get; set; }

    public string? UserId { get; set; }                      // FK -> AspNetUsers (1-1)

    public ICollection<ClassSection> Classes { get; set; } = new List<ClassSection>();
}
