using EduTrack.Application.DTOs.Auth.Student;
using EduTrack.Application.DTOs;
using EduTrack.Domain.Entities;
using EduTrack.Application.Interfaces;
namespace EduTrack.Application.Services;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _repository;

    public StudentService(IStudentRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<StudentDto>> GetAllAsync()
    {
        var students = await _repository.GetAllAsync();
        return students.Select(s => new StudentDto
        {
            Id = s.Id,
            StudentCode = s.StudentCode,
            FullName = s.FullName,
            DateOfBirth = s.DateOfBirth,
            Gender = s.Gender,
            Cohort = s.Cohort,
            Email = s.Email,
            Phone = s.Phone,
            DepartmentId = s.DepartmentId,
        }).ToList();
    }

    public async Task<StudentDto?> GetByIdAsync(int id)
    {
        var student = await _repository.GetByIdAsync(id);
        if (student == null) return null;

        return new StudentDto
        {
            Id = student.Id,
            StudentCode = student.StudentCode,
            FullName = student.FullName,
            DateOfBirth = student.DateOfBirth,
            Gender = student.Gender,
            Cohort = student.Cohort,
            Email = student.Email,
            Phone = student.Phone,
            DepartmentId = student.DepartmentId,
        };
    }

    public async Task<StudentDto> CreateAsync(CreateStudentDto dto)
    {
        //VALIDATE NGÀY SINH (DoB không được ở tương lai)
        if (dto.DateOfBirth > DateTime.Today)
        {
            throw new ArgumentException("Ngày sinh không hợp lệ (không được vượt quá ngày hiện tại)!");
        }

        // VALIDATE TRÙNG MÃ SINH VIÊN (No duplicate code)
        var isCodeDuplicate = await _repository.CodeExistsAsync(dto.StudentCode);
        if (isCodeDuplicate)
        {
            throw new ArgumentException($"Mã sinh viên '{dto.StudentCode}' đã tồn tại!");
        }

        // VALIDATE KHOA (Department)
        var exists = await _repository.DepartmentExistsAsync(dto.DepartmentId);
        if (!exists)
        {
            throw new KeyNotFoundException($"DepartmentId {dto.DepartmentId} không tồn tại!");
        }
        // xử lý trùng email
        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            var isEmailDuplicate = await _repository.EmailExistsAsync(dto.Email);
            if (isEmailDuplicate)
            {
                throw new ArgumentException($"Email '{dto.Email}' đã được sử dụng bởi sinh viên khác!");
            }
        }

        // 4. Khởi tạo Entity Student
        var student = new Student
        {
            StudentCode = dto.StudentCode,
            FullName = dto.FullName,
            DateOfBirth = dto.DateOfBirth,
            Gender = dto.Gender,
            Cohort = dto.Cohort,
            Email = dto.Email,
            Phone = dto.Phone,
            DepartmentId = dto.DepartmentId,
        };

        await _repository.AddAsync(student);
        await _repository.SaveChangesAsync();

        // 5. Trả về DTO
        return new StudentDto
        {
            Id = student.Id,
            StudentCode = student.StudentCode,
            FullName = student.FullName,
            DateOfBirth = student.DateOfBirth,
            Gender = student.Gender,
            Cohort = student.Cohort,
            Email = student.Email,
            Phone = student.Phone,
            DepartmentId = student.DepartmentId,
        };
    }

    public async Task<bool> UpdateAsync(int id, CreateStudentDto dto)
    {
        var student = await _repository.GetByIdAsync(id);
        if (student == null) return false;
        student.StudentCode = dto.StudentCode;
        student.FullName = dto.FullName;
        student.DateOfBirth = dto.DateOfBirth;
        student.Gender = dto.Gender;
        student.Cohort = dto.Cohort;
        student.Email = dto.Email;
        student.Phone = dto.Phone;

        await _repository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var student = await _repository.GetByIdAsync(id);
        if (student == null) return false;

        _repository.Remove(student);
        await _repository.SaveChangesAsync();
        return true;
    }
    // phân trang
    public async Task<(List<StudentDto> Items, int TotalCount)> GetPagedAsync(
    StudentQueryDto query,
    CancellationToken cancellationToken = default)
    {
        // 1. Lấy dữ liệu từ Repository
        var (students, totalCount) = await _repository.GetPagedAsync(query, cancellationToken);

        // 2. Map sang DTO
        var dtos = students.Select(s => new StudentDto
        {
            Id = s.Id,
            StudentCode = s.StudentCode,
            FullName = s.FullName,
            DateOfBirth = s.DateOfBirth,
            Gender = s.Gender,
            Cohort = s.Cohort,
            Email = s.Email,
            Phone = s.Phone,
            DepartmentId = s.DepartmentId,
        }).ToList();

        return (dtos, totalCount);
    }
}