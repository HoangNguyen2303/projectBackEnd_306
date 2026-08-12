using EduTrack.Domain.Common;
using EduTrack.Domain.Enums;

namespace EduTrack.Domain.Entities;

public class Student : BaseEntity
{
    public string StudentCode { get; set; } = string.Empty;  // unique
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string Cohort { get; set; } = string.Empty;       // e.g. K2023
    public string Email { get; set; } = string.Empty;        // unique
    public string? Phone { get; set; }

    public int DepartmentId { get; set; }
    public Department? Department { get; set; }

    public string? UserId { get; set; }                      // FK -> AspNetUsers (1-1)

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
