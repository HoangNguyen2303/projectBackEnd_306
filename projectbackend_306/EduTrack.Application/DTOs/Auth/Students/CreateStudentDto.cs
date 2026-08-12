using EduTrack.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduTrack.Application.DTOs.Auth.Student
{
    public class CreateStudentDto
    {
        [Required(ErrorMessage = "Mã sinh viên không được để trống")]
        public string StudentCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày sinh không được để trống")]
        public DateTime DateOfBirth { get; set; }

        public Gender Gender { get; set; } = Gender.Male;

        public string Cohort { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Khoa")]
        public int DepartmentId { get; set; }
    }
}
