using EduTrack.Application.DTOs;

namespace EduTrack.Application.Interfaces;

public interface IGradeService
{
    /// <summary>Admin/Teacher: xem tất cả điểm.</summary>
    Task<List<GradeDto>> GetAllAsync();

    /// <summary>Student: chỉ xem điểm của chính mình (lọc theo UserId trong token).</summary>
    Task<List<GradeDto>> GetByUserIdAsync(string userId);

    /// <summary>
    /// Lấy 1 bản ghi điểm theo id. requestingUserId + isPrivileged dùng để service tự
    /// kiểm tra quyền sở hữu khi caller là Student (defense-in-depth, không chỉ dựa
    /// vào [Authorize] ở controller). Ném NotFoundException / ForbiddenException.
    /// </summary>
    Task<GradeDto> GetByIdAsync(int gradeId, string requestingUserId, bool isPrivileged);

    /// <summary>
    /// isAdmin=false: chỉ giáo viên phụ trách đúng lớp của enrollment mới được tạo điểm
    /// (§10.3 project doc). Ném ForbiddenException nếu Teacher không phụ trách lớp đó.
    /// </summary>
    Task<GradeDto> CreateAsync(CreateGradeDto dto, string performedByUserId, bool isAdmin);

    /// <summary>isAdmin=false: chỉ giáo viên phụ trách đúng lớp mới được sửa điểm.</summary>
    Task<GradeDto> UpdateAsync(int gradeId, UpdateGradeDto dto, string performedByUserId, bool isAdmin);

    /// <summary>isAdmin=false: chỉ giáo viên phụ trách đúng lớp mới được xoá điểm.</summary>
    Task DeleteAsync(int gradeId, string performedByUserId, bool isAdmin);

    /// <summary>
    /// GET /api/students/{id}/grades — toàn bộ bảng điểm của 1 sinh viên.
    /// requestingUserId + isPrivileged: cùng kiểu kiểm tra quyền sở hữu như GetByIdAsync
    /// (Admin/Teacher xem ai cũng được, Student chỉ xem của chính mình).
    /// Ném NotFoundException nếu không có student đó / ForbiddenException nếu không có quyền.
    /// </summary>
    Task<List<GradeDto>> GetGradesByStudentAsync(int studentId, string requestingUserId, bool isPrivileged);

    /// <summary>
    /// GET /api/students/{id}/gpa — GPA theo §10.3: Σ(Course.Credits × Total) / Σ(Course.Credits),
    /// join Grade → Enrollment → ClassSection → Course để lấy Credits.
    /// Cùng kiểu kiểm tra quyền sở hữu như GetByIdAsync.
    /// </summary>
    Task<GpaDto> GetGpaByStudentAsync(int studentId, string requestingUserId, bool isPrivileged);
}
