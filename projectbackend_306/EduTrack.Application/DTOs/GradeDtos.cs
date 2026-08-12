using System.ComponentModel.DataAnnotations;

namespace EduTrack.Application.DTOs;

/// <summary>Dữ liệu trả về cho client.</summary>
public class GradeDto
{
    public int Id { get; set; }
    public int EnrollmentId { get; set; }

    // Lấy kèm từ Enrollment -> Student / Class để FE không phải gọi thêm API.
    public int StudentId { get; set; }
    public string? StudentName { get; set; }
    public int ClassId { get; set; }
    public string? ClassCode { get; set; }

    public decimal Attendance { get; set; }
    public decimal Midterm { get; set; }
    public decimal Final { get; set; }
    public decimal Total { get; set; }
    public string Classification { get; set; } = string.Empty;
}

/// <summary>Dữ liệu client gửi lên khi tạo điểm mới.</summary>
public class CreateGradeDto
{
    [Required]
    public int EnrollmentId { get; set; }

    [Required]
    [Range(0, 10)]
    public decimal Attendance { get; set; }

    [Required]
    [Range(0, 10)]
    public decimal Midterm { get; set; }

    [Required]
    [Range(0, 10)]
    public decimal Final { get; set; }
}

/// <summary>Dữ liệu client gửi lên khi sửa điểm.</summary>
public class UpdateGradeDto
{
    [Required]
    [Range(0, 10)]
    public decimal Attendance { get; set; }

    [Required]
    [Range(0, 10)]
    public decimal Midterm { get; set; }

    [Required]
    [Range(0, 10)]
    public decimal Final { get; set; }
}

/// <summary>GET /api/students/{id}/gpa — kết quả GPA theo §10.3.</summary>
public class GpaDto
{
    public int StudentId { get; set; }

    /// <summary>GPA = Σ(Course.Credits × Grade.Total) / Σ(Course.Credits), chỉ tính các môn đã có điểm.</summary>
    public decimal Gpa { get; set; }

    public string Classification { get; set; } = string.Empty;
}
