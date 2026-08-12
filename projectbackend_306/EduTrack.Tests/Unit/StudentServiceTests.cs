using Moq;
using Xunit;
using EduTrack.Application.DTOs.Auth.Student;
using EduTrack.Application.Services;
using EduTrack.Application.Interfaces;

namespace EduTrack.Tests;

public class StudentServiceTests
{
    private readonly Mock<IStudentRepository> _repositoryMock;
    private readonly StudentService _studentService;

    public StudentServiceTests()
    {
        // Giả lập Repository bằng Moq
        _repositoryMock = new Mock<IStudentRepository>();
        _studentService = new StudentService(_repositoryMock.Object);
    }

    // 🧪 TEST CASE 1: Báo lỗi khi Mã sinh viên bị trùng
    [Fact]
    public async Task CreateAsync_ShouldThrowException_WhenStudentCodeIsDuplicate()
    {
        // Arrange (Chuẩn bị dữ liệu)
        var dto = new CreateStudentDto
        {
            StudentCode = "SV001",
            FullName = "Nguyen Van A",
            DateOfBirth = new DateTime(2002, 1, 1)
        };

        // Giả lập repository báo mã "SV001" ĐÃ TỒN TẠI (trả về true)
        _repositoryMock
            .Setup(r => r.CodeExistsAsync(dto.StudentCode, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert (Thực thi & Kiểm tra xem có văng lỗi ArgumentException không)
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _studentService.CreateAsync(dto)
        );

        Assert.Contains("đã tồn tại", exception.Message);
    }

    // 🧪 TEST CASE 2: Báo lỗi khi Ngày sinh nằm ở tương lai
    [Fact]
    public async Task CreateAsync_ShouldThrowException_WhenDateOfBirthIsInFuture()
    {
        // Arrange (Chuẩn bị dữ liệu ngày sinh ở tương lai)
        var dto = new CreateStudentDto
        {
            StudentCode = "SV002",
            FullName = "Tran Van B",
            DateOfBirth = DateTime.UtcNow.AddDays(10) // 👈 Ngày ở tương lai
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _studentService.CreateAsync(dto)
        );

        Assert.Contains("Ngày sinh không hợp lệ", exception.Message);
    }
}