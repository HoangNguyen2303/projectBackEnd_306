using EduTrack.Domain.Common;

namespace EduTrack.Domain.Entities;

public class Department : BaseEntity
{
    public string Code { get; set; } = string.Empty;   // unique
    public string Name { get; set; } = string.Empty;

    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
