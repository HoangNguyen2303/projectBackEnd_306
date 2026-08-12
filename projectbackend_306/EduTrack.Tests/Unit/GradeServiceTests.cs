using EduTrack.Application.Common;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using EduTrack.Infrastructure.Data;
using EduTrack.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Tests.Unit;

public class GradeServiceTests
{
    private static AppDbContext CreateInMemoryDb(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }

    /// <summary>
    /// Student ST001 (UserId "user-1") has two graded classes:
    /// C# (4 credits, Total 9.0) and Toan (3 credits, Total 8.0).
    /// GPA = (4*9 + 3*8) / (4+3) = 60/7 ≈ 8.57 -> Gioi.
    /// </summary>
    private static GradeService BuildServiceWithSeededDb(string dbName, out AppDbContext context)
    {
        context = CreateInMemoryDb(dbName);

        var deptIT = new Department { Id = 1, Code = "IT", Name = "Cong nghe thong tin" };
        context.Departments.Add(deptIT);

        var student1 = new Student { Id = 1, StudentCode = "ST001", FullName = "Sinh Vien A", DateOfBirth = new DateTime(2003, 1, 1), Gender = Gender.Male, Cohort = "K2023", Email = "a@edu.vn", DepartmentId = 1, UserId = "user-1" };
        var student2 = new Student { Id = 2, StudentCode = "ST002", FullName = "Sinh Vien B", DateOfBirth = new DateTime(2003, 2, 2), Gender = Gender.Female, Cohort = "K2023", Email = "b@edu.vn", DepartmentId = 1, UserId = "user-2" };
        context.Students.AddRange(student1, student2);

        var teacher = new Teacher { Id = 1, TeacherCode = "TC001", FullName = "Giang Vien X", Email = "x@edu.vn", DepartmentId = 1 };
        context.Teachers.Add(teacher);

        var courseCSharp = new Course { Id = 1, CourseCode = "CS101", Name = "Lap trinh C#", Credits = 4, DepartmentId = 1 };
        var courseMath = new Course { Id = 2, CourseCode = "MA101", Name = "Toan cao cap", Credits = 3, DepartmentId = 1 };
        context.Courses.AddRange(courseCSharp, courseMath);

        var classCS = new ClassSection { Id = 1, ClassCode = "CS101-01", CourseId = 1, TeacherId = 1, Semester = "2026-Q3", MaxCapacity = 40 };
        var classMA = new ClassSection { Id = 2, ClassCode = "MA101-01", CourseId = 2, TeacherId = 1, Semester = "2026-Q3", MaxCapacity = 40 };
        context.Classes.AddRange(classCS, classMA);

        var enrollment1 = new Enrollment { Id = 1, StudentId = 1, ClassId = 1 };
        var enrollment2 = new Enrollment { Id = 2, StudentId = 1, ClassId = 2 };
        context.Enrollments.AddRange(enrollment1, enrollment2);

        context.Grades.AddRange(
            new Grade { Id = 1, EnrollmentId = 1, Attendance = 9, Midterm = 9, Final = 9, Total = 9m, Classification = Classification.Gioi },
            new Grade { Id = 2, EnrollmentId = 2, Attendance = 8, Midterm = 8, Final = 8, Total = 8m, Classification = Classification.Gioi });

        context.SaveChanges();

        return new GradeService(context);
    }

    [Fact]
    public async Task GetGradesByStudentAsync_AsSelf_ReturnsOwnGrades()
    {
        var service = BuildServiceWithSeededDb(nameof(GetGradesByStudentAsync_AsSelf_ReturnsOwnGrades), out _);

        var grades = await service.GetGradesByStudentAsync(studentId: 1, requestingUserId: "user-1", isPrivileged: false);

        Assert.Equal(2, grades.Count);
        Assert.All(grades, g => Assert.Equal(1, g.StudentId));
    }

    [Fact]
    public async Task GetGradesByStudentAsync_AsAdminOrTeacher_ReturnsAnyStudentsGrades()
    {
        var service = BuildServiceWithSeededDb(nameof(GetGradesByStudentAsync_AsAdminOrTeacher_ReturnsAnyStudentsGrades), out _);

        var grades = await service.GetGradesByStudentAsync(studentId: 1, requestingUserId: "some-admin-user", isPrivileged: true);

        Assert.Equal(2, grades.Count);
    }

    [Fact]
    public async Task GetGradesByStudentAsync_AsOtherStudent_ThrowsForbidden()
    {
        var service = BuildServiceWithSeededDb(nameof(GetGradesByStudentAsync_AsOtherStudent_ThrowsForbidden), out _);

        await Assert.ThrowsAsync<ForbiddenException>(
            () => service.GetGradesByStudentAsync(studentId: 1, requestingUserId: "user-2", isPrivileged: false));
    }

    [Fact]
    public async Task GetGradesByStudentAsync_UnknownStudent_ThrowsNotFound()
    {
        var service = BuildServiceWithSeededDb(nameof(GetGradesByStudentAsync_UnknownStudent_ThrowsNotFound), out _);

        await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetGradesByStudentAsync(studentId: 999, requestingUserId: "user-1", isPrivileged: false));
    }

    [Fact]
    public async Task GetGpaByStudentAsync_WeightsCreditsCorrectly_AndClassifiesAsGioi()
    {
        var service = BuildServiceWithSeededDb(nameof(GetGpaByStudentAsync_WeightsCreditsCorrectly_AndClassifiesAsGioi), out _);

        // GPA = (4*9 + 3*8) / (4+3) = 60/7 = 8.57 (rounded) -> Gioi (>= 8.0)
        var gpa = await service.GetGpaByStudentAsync(studentId: 1, requestingUserId: "user-1", isPrivileged: false);

        Assert.Equal(8.57m, gpa.Gpa);
        Assert.Equal(Classification.Gioi.ToString(), gpa.Classification);
    }

    [Fact]
    public async Task GetGpaByStudentAsync_ClassificationBoundary_JustBelowGioiIsKha()
    {
        var context = CreateInMemoryDb(nameof(GetGpaByStudentAsync_ClassificationBoundary_JustBelowGioiIsKha));

        var student = new Student { Id = 1, StudentCode = "ST001", FullName = "Sinh Vien A", DateOfBirth = new DateTime(2003, 1, 1), Gender = Gender.Male, Cohort = "K2023", Email = "a@edu.vn", DepartmentId = 1, UserId = "user-1" };
        context.Departments.Add(new Department { Id = 1, Code = "IT", Name = "CNTT" });
        context.Students.Add(student);
        context.Teachers.Add(new Teacher { Id = 1, TeacherCode = "TC001", FullName = "GV X", Email = "x@edu.vn", DepartmentId = 1 });
        var course = new Course { Id = 1, CourseCode = "CS101", Name = "Lap trinh C#", Credits = 4, DepartmentId = 1 };
        context.Courses.Add(course);
        var classSection = new ClassSection { Id = 1, ClassCode = "CS101-01", CourseId = 1, TeacherId = 1, Semester = "2026-Q3", MaxCapacity = 40 };
        context.Classes.Add(classSection);
        var enrollment = new Enrollment { Id = 1, StudentId = 1, ClassId = 1 };
        context.Enrollments.Add(enrollment);
        context.Grades.Add(new Grade { Id = 1, EnrollmentId = 1, Attendance = 7.9m, Midterm = 7.9m, Final = 7.9m, Total = 7.9m, Classification = Classification.Kha });
        context.SaveChanges();

        var service = new GradeService(context);

        var gpa = await service.GetGpaByStudentAsync(studentId: 1, requestingUserId: "user-1", isPrivileged: false);

        Assert.Equal(7.9m, gpa.Gpa);
        Assert.Equal(Classification.Kha.ToString(), gpa.Classification);
    }

    [Fact]
    public async Task GetGpaByStudentAsync_AsOtherStudent_ThrowsForbidden()
    {
        var service = BuildServiceWithSeededDb(nameof(GetGpaByStudentAsync_AsOtherStudent_ThrowsForbidden), out _);

        await Assert.ThrowsAsync<ForbiddenException>(
            () => service.GetGpaByStudentAsync(studentId: 1, requestingUserId: "user-2", isPrivileged: false));
    }

    [Fact]
    public async Task GetGpaByStudentAsync_UnknownStudent_ThrowsNotFound()
    {
        var service = BuildServiceWithSeededDb(nameof(GetGpaByStudentAsync_UnknownStudent_ThrowsNotFound), out _);

        await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetGpaByStudentAsync(studentId: 999, requestingUserId: "user-1", isPrivileged: false));
    }
}
