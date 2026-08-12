using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduTrack.Application.DTOs.Auth.Teacher
{
    public class CreateTeacherDto
    {
        [Required(ErrorMessage = "Mã giảng viên không được để trống")]
        public string TeacherCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên giảng viên không được để trống")]
        public string FullName { get; set; } = string.Empty;

        public string? Specialization { get; set; } // Thêm Chuyên môn (có thể để trống)

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn Khoa cho giảng viên")]
        public int DepartmentId { get; set; }
    }
}
