using EduTrack.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduTrack.Application.DTOs.Auth.Student
{
    public class StudentDto

    {
        // Kế thừa từ BaseEntity nên ta lấy Id ra đây
        public int Id { get; set; }

        public string StudentCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }

        // Kiểu chuỗi hoặc Enum để trả về giới tính cho Front-end dễ hiển thị
        public Gender Gender { get; set; }

        public string Cohort { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }


        public int DepartmentId { get; set; }


        // Mối quan hệ với User (nếu cần hiển thị)
        public string? UserId { get; set; }
    }
}
