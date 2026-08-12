using EduTrack.Domain.Common;

namespace EduTrack.Domain.Entities;

/// <summary>Join entity: a student enrolled in a class section (many-to-many).</summary>
public class Enrollment : BaseEntity
{
    public int StudentId { get; set; }
    public Student? Student { get; set; }

    public int ClassId { get; set; }
    public ClassSection? Class { get; set; }

    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    public Grade? Grade { get; set; }                        // 1-1
}
