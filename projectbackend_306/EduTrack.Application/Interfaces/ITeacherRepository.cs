using EduTrack.Application.DTOs.Auth.Teacher;
using EduTrack.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduTrack.Application.Interfaces
{
    public interface ITeacherRepository
    {
        Task<List<Teacher>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Teacher?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task AddAsync(Teacher teacher, CancellationToken cancellationToken = default);
        void Remove(Teacher teacher);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<(IReadOnlyList<Teacher> Items, int TotalCount)> GetPagedAsync(
        TeacherQueryDto query,
        CancellationToken cancellationToken = default);
        Task<bool> DepartmentExistsAsync(int departmentId, CancellationToken cancellationToken = default);
    }
}
