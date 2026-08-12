using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduTrack.Application.DTOs.Auth.Teacher
{
    public class TeacherDto
    {
        public int Id { get; set; }
        public string TeacherCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string? DepartmentName { get; set; }
        public string? UserId { get; set; }
    }
}
