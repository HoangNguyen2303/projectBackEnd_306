using EduTrack.Domain.Common;

namespace EduTrack.Domain.Entities;

/// <summary>A class section (lớp học phần): an offering of a course in a semester.</summary>
public class ClassSection : BaseEntity
{
    public string ClassCode { get; set; } = string.Empty;    // unique
    public string Semester { get; set; } = string.Empty;     // e.g. 2025-Q3
    public int MaxCapacity { get; set; }                     // > 0

    public int CourseId { get; set; }
    public Course? Course { get; set; }

    public int TeacherId { get; set; }
    public Teacher? Teacher { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
