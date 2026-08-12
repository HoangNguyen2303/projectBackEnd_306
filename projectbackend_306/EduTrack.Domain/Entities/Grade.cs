using EduTrack.Domain.Common;
using EduTrack.Domain.Enums;

namespace EduTrack.Domain.Entities;

public class Grade : BaseEntity
{
    public int EnrollmentId { get; set; }
    public Enrollment? Enrollment { get; set; }

    public decimal Attendance { get; set; }   // 0..10 (chuyên cần)
    public decimal Midterm { get; set; }      // 0..10 (giữa kỳ)
    public decimal Final { get; set; }        // 0..10 (cuối kỳ)

    public decimal Total { get; set; }        // computed in service layer
    public Classification Classification { get; set; }
}
