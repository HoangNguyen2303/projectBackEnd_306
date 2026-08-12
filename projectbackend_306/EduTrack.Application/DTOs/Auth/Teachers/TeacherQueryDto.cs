using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduTrack.Application.DTOs.Auth.Teacher
{
    public class TeacherQueryDto
    {
        public string? Search { get; set; }
        public string? TeacherCode { get; set; }
        public bool? IsActive { get; set; }

        // Phân trang (Mặc định trang 1, mỗi trang 20 phần tử giống ClassQueryDto)
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
