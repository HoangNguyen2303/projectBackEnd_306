using EduTrack.Application.DTOs.Auth.Teacher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduTrack.Application.Interfaces
{
    public interface ITeacherService
    {
        Task<IEnumerable<TeacherDto>> GetAllAsync();
        Task<TeacherDto?> GetByIdAsync(int id);
        Task<TeacherDto> CreateAsync(CreateTeacherDto dto);
        Task<bool> UpdateAsync(int id, CreateTeacherDto dto);
        Task<bool> DeleteAsync(int id);
        Task<(List<TeacherDto> Items, int TotalCount)> GetPagedAsync(
        TeacherQueryDto query,
        CancellationToken cancellationToken = default);
    }
}
