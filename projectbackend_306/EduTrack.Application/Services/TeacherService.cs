using EduTrack.Application.DTOs.Auth.Student;
using EduTrack.Application.DTOs.Auth.Teacher;
using EduTrack.Application.Interfaces;
using EduTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly ITeacherRepository _repository;

        public TeacherService(ITeacherRepository repository)
        {
            _repository = repository;
        }
        //Lấy tất cả giảng viên
        public async Task<IEnumerable<TeacherDto>> GetAllAsync()
        {
            var teachers = await _repository.GetAllAsync();
            return teachers.Select(t => new TeacherDto
            {
                Id = t.Id,
                TeacherCode = t.TeacherCode,
                FullName = t.FullName,
                Specialization = t.Specialization,
                Email = t.Email,
                DepartmentName = t.Department?.Name,
                UserId = t.UserId
            }).ToList();
        }

        //tìm giảng viên theo ID
        public async Task<TeacherDto?> GetByIdAsync(int id)
        {
            var teachers = await _repository.GetByIdAsync(id);
            if (teachers == null) return null;

            return new TeacherDto
            {
                Id = teachers.Id,
                TeacherCode = teachers.TeacherCode,
                FullName = teachers.FullName,
                Specialization = teachers.Specialization,
                Email = teachers.Email,
                DepartmentName = teachers.Department?.Name,
                UserId = teachers.UserId
            };
        }
        // Thêm mới giảng viên
        public async Task<TeacherDto> CreateAsync(CreateTeacherDto dto)
        {
            var exists = await _repository.DepartmentExistsAsync(dto.DepartmentId);
            if (!exists)
            {
                throw new KeyNotFoundException($"DepartmentId {dto.DepartmentId} không tồn tại!");
            }
            var teacher = new Teacher
            {
                TeacherCode = dto.TeacherCode,
                FullName = dto.FullName,
                Specialization = dto.Specialization,
                Email = dto.Email,
                DepartmentId = dto.DepartmentId,

            };

            await _repository.AddAsync(teacher);
            await _repository.SaveChangesAsync();
            // trả về giá trị đã thêm
            return new TeacherDto
            {
                Id = teacher.Id,
                TeacherCode = teacher.TeacherCode,
                FullName = teacher.FullName,
                Specialization = teacher.Specialization,
                Email = teacher.Email,
                DepartmentName = teacher.Department?.Name,
                UserId = teacher.UserId
            };
        }
        // Cập Nhật Thông tin giảng viên
        public async Task<bool> UpdateAsync(int id, CreateTeacherDto dto)
        {
            var teacher = await _repository.GetByIdAsync(id);
            if (teacher == null) return false;

            teacher.TeacherCode = dto.TeacherCode;
            teacher.FullName = dto.FullName;
            teacher.Specialization = dto.Specialization;
            teacher.Email = dto.Email;
            teacher.DepartmentId = dto.DepartmentId;


            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var teacher = await _repository.GetByIdAsync(id);
            if (teacher == null) return false;

            _repository.Remove(teacher);
            await _repository.SaveChangesAsync();
            return true;
        }

        // phân trang
        public async Task<(List<TeacherDto> Items, int TotalCount)> GetPagedAsync(
        TeacherQueryDto query,
        CancellationToken cancellationToken = default)
        {
            // 1. Lấy dữ liệu từ Repository
            var (teachers, totalCount) = await _repository.GetPagedAsync(query, cancellationToken);

            // 2. Mapping sang DTO
            var dtos = teachers.Select(s => new TeacherDto
            {
                Id = s.Id,
                TeacherCode = s.TeacherCode,
                FullName = s.FullName,
                Specialization = s.Specialization,
                Email = s.Email,
                DepartmentName = s.Department?.Name,
                UserId = s.UserId
            }).ToList();

            return (dtos, totalCount);
        }
    }
}