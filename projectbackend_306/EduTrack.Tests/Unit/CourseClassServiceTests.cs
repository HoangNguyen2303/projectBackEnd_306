using EduTrack.Application.DTOs.Classes;
using EduTrack.Application.DTOs.Courses;
using EduTrack.Application.Interfaces;
using EduTrack.Application.Services;
using EduTrack.Application.Validators;
using EduTrack.Domain.Entities;
using Moq;

namespace EduTrack.Tests.Unit;

public sealed class CourseClassServiceTests
{
    [Fact]
    public async Task CreateCourseAsync_WhenCodeExists_ReturnsConflict()
    {
        var repository = new Mock<ICourseRepository>();
        repository
            .Setup(x => x.CodeExistsAsync("CSW306", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var service = CreateCourseService(repository.Object);

        var result = await service.CreateAsync(new CreateCourseDto
        {
            CourseCode = "csw306",
            Name = "Backend Development",
            Credits = 4,
            DepartmentId = 1
        });

        Assert.False(result.Success);
        Assert.Equal(409, result.StatusCode);
        Assert.Equal("Course code already exists.", result.Message);
        repository.Verify(
            x => x.AddAsync(It.IsAny<Course>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteCourseAsync_WhenCourseHasClasses_ReturnsConflict()
    {
        var repository = new Mock<ICourseRepository>();
        repository
            .Setup(x => x.GetByIdAsync(7, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Course { Id = 7, CourseCode = "CSW306" });
        repository
            .Setup(x => x.HasClassesAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var service = CreateCourseService(repository.Object);

        var result = await service.DeleteAsync(7);

        Assert.False(result.Success);
        Assert.Equal(409, result.StatusCode);
        Assert.Equal("Cannot delete a course that has class sections.", result.Message);
        repository.Verify(x => x.Remove(It.IsAny<Course>()), Times.Never);
    }

    [Fact]
    public async Task UpdateClassAsync_WhenCapacityIsBelowEnrollmentCount_ReturnsConflict()
    {
        var classSection = new ClassSection
        {
            Id = 9,
            ClassCode = "CSW306-A",
            Semester = "2026-Q3",
            MaxCapacity = 10,
            CourseId = 1,
            TeacherId = 1,
            Enrollments = new List<Enrollment>
            {
                new() { Id = 1, StudentId = 1, ClassId = 9 },
                new() { Id = 2, StudentId = 2, ClassId = 9 }
            }
        };
        var repository = new Mock<IClassRepository>();
        repository
            .Setup(x => x.GetByIdAsync(9, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(classSection);
        var service = CreateClassService(repository.Object);

        var result = await service.UpdateAsync(9, new UpdateClassDto
        {
            ClassCode = "CSW306-A",
            CourseId = 1,
            TeacherId = 1,
            Semester = "2026-Q3",
            MaxCapacity = 1
        });

        Assert.False(result.Success);
        Assert.Equal(409, result.StatusCode);
        Assert.Equal(
            "Max capacity cannot be lower than the current enrollment count.",
            result.Message);
        repository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetRosterAsync_WhenTeacherDoesNotOwnClass_ReturnsForbidden()
    {
        var repository = new Mock<IClassRepository>();
        repository
            .Setup(x => x.GetWithRosterAsync(4, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ClassSection
            {
                Id = 4,
                ClassCode = "CSW306-A",
                MaxCapacity = 30,
                Teacher = new Teacher { Id = 2, UserId = "teacher-owner" }
            });
        var service = CreateClassService(repository.Object);

        var result = await service.GetRosterAsync(
            4,
            currentUserId: "another-teacher",
            isAdmin: false);

        Assert.False(result.Success);
        Assert.Equal(403, result.StatusCode);
        Assert.Equal(
            "Teachers may view only the roster of classes assigned to them.",
            result.Message);
    }

    private static CourseService CreateCourseService(ICourseRepository repository)
        => new(
            repository,
            new CourseQueryValidator(),
            new CreateCourseValidator(),
            new UpdateCourseValidator());

    private static ClassService CreateClassService(IClassRepository repository)
        => new(
            repository,
            new ClassQueryValidator(),
            new CreateClassValidator(),
            new UpdateClassValidator());
}
