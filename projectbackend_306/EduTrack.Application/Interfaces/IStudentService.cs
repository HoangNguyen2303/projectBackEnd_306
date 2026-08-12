using EduTrack.Application.DTOs.Auth.Student;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduTrack.Application.Interfaces
{
    public interface IStudentService
    {
        Task<IEnumerable<StudentDto>> GetAllAsync();
        Task<StudentDto?> GetByIdAsync(int id);
        Task<StudentDto> CreateAsync(CreateStudentDto dto);
        Task<bool> UpdateAsync(int id, CreateStudentDto dto);
        Task<bool> DeleteAsync(int id);
        Task<(List<StudentDto> Items, int TotalCount)> GetPagedAsync(
        StudentQueryDto query,
        CancellationToken cancellationToken = default);
    }
}
