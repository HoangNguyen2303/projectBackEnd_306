using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using EduTrack.Infrastructure.Data;
using EduTrack.Infrastructure.Identity;
using EduTrack.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Tests.Unit;

public class StatisticsServiceTests
{
    private static AppDbContext CreateInMemoryDb(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }

    private static StatisticsService BuildServiceWithSeededDb(string dbName, out AppDbContext context)
    {
        context = CreateInMemoryDb(dbName);
        SeedTestData(context);
        return new StatisticsService(context);
    }

    private static void SeedTestData(AppDbContext db)
    {
        var deptIT = new Department { Id = 1, Code = "IT", Name = "Công nghệ Thông tin" };
        var deptBiz = new Department { Id = 2, Code = "BUS", Name = "Quản trị Kinh doanh" };
        db.Departments.AddRange(deptIT, deptBiz);

        var students = new List<Student>
        {
            new() { Id = 1, StudentCode = "ST001", FullName = "Sinh Vien A", DateOfBirth = new DateTime(2003,1,1), Gender = Gender.Male, Cohort = "K2023", Email = "a@edu.vn", DepartmentId = 1 },
            new() { Id = 2, StudentCode = "ST002", FullName = "Sinh Vien B", DateOfBirth = new DateTime(2003,2,2), Gender = Gender.Female, Cohort = "K2023", Email = "b@edu.vn", DepartmentId = 1 },
            new() { Id = 3, StudentCode = "ST003", FullName = "Sinh Vien C", DateOfBirth = new DateTime(2003,3,3), Gender = Gender.Male, Cohort = "K2023", Email = "c@edu.vn", DepartmentId = 1 },
            new() { Id = 4, StudentCode = "ST004", FullName = "Sinh Vien D", DateOfBirth = new DateTime(2003,4,4), Gender = Gender.Female, Cohort = "K2023", Email = "d@edu.vn", DepartmentId = 1 },
            new() { Id = 5, StudentCode = "ST005", FullName = "Sinh Vien E", DateOfBirth = new DateTime(2003,5,5), Gender = Gender.Male, Cohort = "K2023", Email = "e@edu.vn", DepartmentId = 1 },
            new() { Id = 6, StudentCode = "ST006", FullName = "Sinh Vien F", DateOfBirth = new DateTime(2003,6,6), Gender = Gender.Female, Cohort = "K2023", Email = "f@edu.vn", DepartmentId = 2 },
            new() { Id = 7, StudentCode = "ST007", FullName = "Sinh Vien G", DateOfBirth = new DateTime(2003,7,7), Gender = Gender.Male, Cohort = "K2023", Email = "g@edu.vn", DepartmentId = 2 },
            new() { Id = 8, StudentCode = "ST008", FullName = "Sinh Vien H", DateOfBirth = new DateTime(2003,8,8), Gender = Gender.Female, Cohort = "K2023", Email = "h@edu.vn", DepartmentId = 2 }
        };
        db.Students.AddRange(students);

        var teacher = new Teacher { Id = 1, TeacherCode = "TC001", FullName = "Giang Vien X", Email = "x@edu.vn", DepartmentId = 1 };
        db.Teachers.Add(teacher);

        var courseCSharp = new Course { Id = 1, CourseCode = "CS101", Name = "Lap trinh C#", Credits = 4, DepartmentId = 1 };
        var courseMath = new Course { Id = 2, CourseCode = "MA101", Name = "Toan cao cap", Credits = 3, DepartmentId = 1 };
        db.Courses.AddRange(courseCSharp, courseMath);

        var classCS = new ClassSection { Id = 1, ClassCode = "CS101-2025-Q3-01", CourseId = 1, TeacherId = 1, Semester = "2025-Q3", MaxCapacity = 40 };
        var classMA = new ClassSection { Id = 2, ClassCode = "MA101-2025-Q3-01", CourseId = 2, TeacherId = 1, Semester = "2025-Q3", MaxCapacity = 40 };
        db.Classes.AddRange(classCS, classMA);

        var enrollments = new List<Enrollment>
        {
            new() { Id = 1, StudentId = 1, ClassId = 1 },
            new() { Id = 2, StudentId = 2, ClassId = 1 },
            new() { Id = 3, StudentId = 3, ClassId = 1 },
            new() { Id = 4, StudentId = 6, ClassId = 1 },
            new() { Id = 5, StudentId = 7, ClassId = 1 },
            new() { Id = 6, StudentId = 1, ClassId = 2 },
            new() { Id = 7, StudentId = 2, ClassId = 2 },
            new() { Id = 8, StudentId = 6, ClassId = 2 }
        };
        db.Enrollments.AddRange(enrollments);

        var grades = new List<Grade>
        {
            // ST001 - C# (4 credits): 9.0 (Tổng = 9) - MA (3 credits): 8.0 (Tổng = 8)
            // GPA = (4*9 + 3*8) / (4+3) = (36+24)/7 = 60/7 ≈ 8.57 -> Gioi
            new() { Id = 1, EnrollmentId = 1, Attendance = 9, Midterm = 9, Final = 9, Total = 9m, Classification = Classification.Gioi },
            new() { Id = 2, EnrollmentId = 6, Attendance = 8, Midterm = 8, Final = 8, Total = 8, Classification = Classification.Gioi },

            // ST002 - C# (4): 7.0, MA (3): 7.0
            // GPA = (4*7 + 3*7)/7 = 49/7 = 7.0 -> Kha
            new() { Id = 3, EnrollmentId = 2, Attendance = 7, Midterm = 7, Final = 7, Total = 7, Classification = Classification.Kha },
            new() { Id = 4, EnrollmentId = 7, Attendance = 7, Midterm = 7, Final = 7, Total = 7, Classification = Classification.Kha },

            // ST003 - C# (4): 4.0 (Yếu/Rớt) -> Fail
            // Chưa học MA
            new() { Id = 5, EnrollmentId = 3, Attendance = 4, Midterm = 4, Final = 4, Total = 4, Classification = Classification.Yeu },

            // ST006 - C# (4): 4.5 (Rớt), MA (3): 4.0 (Rớt)
            // GPA = (4*4.5 + 3*4)/7 = (18+12)/7 = 30/7 ≈ 4.29 -> Yeu
            new() { Id = 6, EnrollmentId = 4, Attendance = 4.5m, Midterm = 4.5m, Final = 4.5m, Total = 4.5m, Classification = Classification.Yeu },
            new() { Id = 7, EnrollmentId = 8, Attendance = 4, Midterm = 4, Final = 4, Total = 4, Classification = Classification.Yeu },

            // ST007 - C# (4): 8.5 -> Đậu
            new() { Id = 8, EnrollmentId = 5, Attendance = 8.5m, Midterm = 8.5m, Final = 8.5m, Total = 8.5m, Classification = Classification.Gioi }
        };
        db.Grades.AddRange(grades);

        var user = new AppUser { Id = "u1", UserName = "admin@edutrack.edu", Email = "admin@edutrack.edu", FullName = "Admin", Role = UserRole.Admin };
        db.Users.Add(user);

        var auditLogs = new List<AuditLog>
        {
            new() { Id = 1, UserId = "u1", Action = "GRADE_UPDATE", Entity = "Grade#1", OldValue = "8", NewValue = "9", At = new DateTime(2025, 10, 1, 10, 0, 0, DateTimeKind.Utc) },
            new() { Id = 2, UserId = "u1", Action = "GRADE_CREATE", Entity = "Grade#5", OldValue = null, NewValue = "{total:4}", At = new DateTime(2025, 10, 2, 11, 0, 0, DateTimeKind.Utc) }
        };
        db.AuditLogs.AddRange(auditLogs);

        db.SaveChanges();
    }

    [Fact]
    public async Task GetStudentsByDepartment_CountsAddUp()
    {
        var service = BuildServiceWithSeededDb(nameof(GetStudentsByDepartment_CountsAddUp), out _);

        var result = await service.GetStudentsByDepartmentAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal(5, result.First(x => x.DepartmentCode == "IT").StudentCount);
        Assert.Equal(3, result.First(x => x.DepartmentCode == "BUS").StudentCount);
        Assert.Equal(8, result.Sum(x => x.StudentCount));
    }

    [Fact]
    public async Task GetPassFail_SemesterFilter_PercentagesCorrect()
    {
        var service = BuildServiceWithSeededDb(nameof(GetPassFail_SemesterFilter_PercentagesCorrect), out _);

        var result = await service.GetPassFailAsync("2025-Q3");

        var sem = result.First();
        Assert.Equal(8, sem.TotalEnrollments);
        Assert.Equal(5, sem.PassedCount);
        Assert.Equal(3, sem.FailedCount);
        Assert.Equal(Math.Round(5.0 / 8 * 100, 2), sem.PassPercentage);
        Assert.Equal(Math.Round(3.0 / 8 * 100, 2), sem.FailPercentage);
    }

    [Fact]
    public async Task GetTopStudents_GPAAndOrderingCorrect()
    {
        var service = BuildServiceWithSeededDb(nameof(GetTopStudents_GPAAndOrderingCorrect), out _);

        var result = await service.GetTopStudentsAsync(topN: 3);

        Assert.Equal(3, result.Count);
        Assert.True(result[0].Gpa >= result[1].Gpa && result[1].Gpa >= result[2].Gpa, "Top students not ordered correctly");

        var ST001 = result.First(x => x.StudentCode == "ST001");
        Assert.Equal(8.57m, ST001.Gpa);
        Assert.Equal("Giỏi", ST001.Classification);

        var ST007 = result.First(x => x.StudentCode == "ST007");
        Assert.Equal(8.5m, ST007.Gpa);
        Assert.Equal("Giỏi", ST007.Classification);
    }

    [Fact]
    public async Task GetAcademicWarnings_ThresholdAndFailedCoursesCorrect()
    {
        var service = BuildServiceWithSeededDb(nameof(GetAcademicWarnings_ThresholdAndFailedCoursesCorrect), out _);

        var result = await service.GetAcademicWarningsAsync(gpaThreshold: 5.0m);

        Assert.Contains(result, x => x.StudentCode == "ST003" && x.FailedCourseCount == 1);
        var st006 = result.First(x => x.StudentCode == "ST006");
        Assert.Equal(2, st006.FailedCourseCount);
        Assert.Equal("Yếu", st006.Classification);
        Assert.Equal(4.29m, st006.Gpa);
        Assert.Contains("mức 2", st006.WarningLevel);
    }

    [Fact]
    public async Task GetAuditLogs_ReturnsEntriesWithUserEmail()
    {
        var service = BuildServiceWithSeededDb(nameof(GetAuditLogs_ReturnsEntriesWithUserEmail), out _);

        var result = await service.GetAuditLogsAsync();

        Assert.Equal(2, result.Count);
        Assert.All(result, x => Assert.Equal("admin@edutrack.edu", x.UserEmail));
        Assert.Equal("GRADE_CREATE", result[0].Action);
        Assert.Equal("GRADE_UPDATE", result[1].Action);
    }

    [Fact]
    public async Task GetAuditLogs_FromDateFilter_WorksCorrectly()
    {
        var service = BuildServiceWithSeededDb(nameof(GetAuditLogs_FromDateFilter_WorksCorrectly), out _);

        var from = new DateTime(2025, 10, 2, 0, 0, 0, DateTimeKind.Utc);
        var result = await service.GetAuditLogsAsync(from: from);

        Assert.Single(result);
        Assert.Equal("GRADE_CREATE", result[0].Action);
    }
}
