using EduTrack.Domain.Common;

namespace EduTrack.Domain.Entities;

public class Course : BaseEntity
{
    public string CourseCode { get; set; } = string.Empty;   // unique
    public string Name { get; set; } = string.Empty;
    public int Credits { get; set; }                         // 1..10

    public int DepartmentId { get; set; }
    public Department? Department { get; set; }

    public ICollection<ClassSection> Classes { get; set; } = new List<ClassSection>();
}
