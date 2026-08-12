using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduTrack.Application.DTOs.Auth.Student
{
    public class StudentQueryDto
    {
        // Tìm kiếm theo Tên, Email, Mã sinh viên (MSSV)...
        public string? Search { get; set; }
        // Lọc theo Khoa
        public int? DepartmentId { get; set; }
        public string? SortBy { get; set; } = "Id";
        public bool IsDescending { get; set; } = false;

        // Các trường lọc riêng cho Học sinh (nếu dự án cần, không dùng thì bỏ qua)
        public string? StudentCode { get; set; } // Mã sinh viên
        public bool? IsActive { get; set; }      // Trạng thái còn học hay nghỉ

        // Phân trang (Mặc định trang 1, mỗi trang 20 phần tử giống ClassQueryDto)
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
