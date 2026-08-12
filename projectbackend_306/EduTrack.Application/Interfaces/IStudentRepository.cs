using EduTrack.Application.DTOs.Auth.Student;
using EduTrack.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduTrack.Application.Interfaces
{
    public interface IStudentRepository
    {
        Task<List<Student>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task AddAsync(Student student, CancellationToken cancellationToken = default);
        void Remove(Student student);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<(IReadOnlyList<Student> Items, int TotalCount)> GetPagedAsync(
        StudentQueryDto query,
        CancellationToken cancellationToken = default);
        // kiểm tra department có hay không
        Task<bool> DepartmentExistsAsync(int departmentId, CancellationToken cancellationToken = default);
        // kiểm tra dob nhập vào
        Task<bool> CodeExistsAsync(string studentCode, int? excludeId = null, CancellationToken cancellationToken = default);
        // kiểm tra email
        Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    }
}
