using EduTrack.Application.DTOs.Enrollments;
using EduTrack.Application.Interfaces;
using EduTrack.Application.Services;
using EduTrack.Application.Validators;
using EduTrack.Domain.Entities;

namespace EduTrack.Tests.Unit;

public sealed class EnrollmentServiceTests
{
    [Fact]
    public async Task GetAsync_AsStudent_ReturnsOnlyOwnEnrollments()
    {
        var currentStudent = CreateStudent(1, "S001");
        var otherStudent = CreateStudent(2, "S002");
        var classSection = CreateClass(maxCapacity: 10);
        var ownEnrollment = CreateEnrollment(10, currentStudent, classSection);
        var otherEnrollment = CreateEnrollment(11, otherStudent, classSection);
        var repository = new FakeEnrollmentRepository(
            new[] { currentStudent, otherStudent },
            new[] { classSection },
            new[] { ownEnrollment, otherEnrollment });
        var service = CreateService(repository);

        var result = await service.GetAsync(
            new EnrollmentQueryDto(),
            currentUserId: currentStudent.UserId,
            isAdmin: false);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data.TotalCount);
        var item = Assert.Single(result.Data.Items);
        Assert.Equal(currentStudent.Id, item.StudentId);
    }

    [Fact]
    public async Task GetAsync_WhenStudentRequestsAnotherStudent_ReturnsForbidden()
    {
        var currentStudent = CreateStudent(1, "S001");
        var otherStudent = CreateStudent(2, "S002");
        var repository = new FakeEnrollmentRepository(
            new[] { currentStudent, otherStudent },
            Array.Empty<ClassSection>(),
            Array.Empty<Enrollment>());
        var service = CreateService(repository);

        var result = await service.GetAsync(
            new EnrollmentQueryDto { StudentId = otherStudent.Id },
            currentUserId: currentStudent.UserId,
            isAdmin: false);

        Assert.False(result.Success);
        Assert.Equal(403, result.StatusCode);
        Assert.Equal("Students may view only their own enrollments.", result.Message);
    }

    [Fact]
    public async Task EnrollAsync_WhenStudentManagesAnotherStudent_ReturnsForbidden()
    {
        var currentStudent = CreateStudent(1, "S001");
        var otherStudent = CreateStudent(2, "S002");
        var classSection = CreateClass(maxCapacity: 10);
        var repository = new FakeEnrollmentRepository(
            new[] { currentStudent, otherStudent },
            new[] { classSection },
            Array.Empty<Enrollment>());
        var service = CreateService(repository);

        var result = await service.EnrollAsync(
            new CreateEnrollmentDto { StudentId = otherStudent.Id, ClassId = classSection.Id },
            currentUserId: currentStudent.UserId,
            isAdmin: false);

        Assert.False(result.Success);
        Assert.Equal(403, result.StatusCode);
        Assert.Equal("Students may manage only their own enrollments.", result.Message);
    }

    [Fact]
    public async Task EnrollAsync_WhenClassIsFull_ReturnsConflict()
    {
        var targetStudent = CreateStudent(1, "S001");
        var otherStudent = CreateStudent(2, "S002");
        var classSection = CreateClass(maxCapacity: 1);
        var existing = CreateEnrollment(10, otherStudent, classSection);
        var repository = new FakeEnrollmentRepository(
            new[] { targetStudent, otherStudent },
            new[] { classSection },
            new[] { existing });
        var service = CreateService(repository);

        var result = await service.EnrollAsync(
            new CreateEnrollmentDto { StudentId = targetStudent.Id, ClassId = classSection.Id },
            currentUserId: null,
            isAdmin: true);

        Assert.False(result.Success);
        Assert.Equal(409, result.StatusCode);
        Assert.Equal("Class is full.", result.Message);
    }

    [Fact]
    public async Task EnrollAsync_WhenEnrollmentAlreadyExists_ReturnsConflict()
    {
        var student = CreateStudent(1, "S001");
        var classSection = CreateClass(maxCapacity: 10);
        var existing = CreateEnrollment(10, student, classSection);
        var repository = new FakeEnrollmentRepository(
            new[] { student },
            new[] { classSection },
            new[] { existing });
        var service = CreateService(repository);

        var result = await service.EnrollAsync(
            new CreateEnrollmentDto { StudentId = student.Id, ClassId = classSection.Id },
            currentUserId: null,
            isAdmin: true);

        Assert.False(result.Success);
        Assert.Equal(409, result.StatusCode);
        Assert.Equal("Student is already enrolled in this class.", result.Message);
    }

    [Fact]
    public async Task DropThenEnroll_FreesSeatAndAllowsEnrollmentAgain()
    {
        var student = CreateStudent(1, "S001");
        var classSection = CreateClass(maxCapacity: 1);
        var existing = CreateEnrollment(10, student, classSection);
        var repository = new FakeEnrollmentRepository(
            new[] { student },
            new[] { classSection },
            new[] { existing });
        var service = CreateService(repository);

        var dropResult = await service.DropAsync(
            existing.Id,
            currentUserId: null,
            isAdmin: true);
        var enrollResult = await service.EnrollAsync(
            new CreateEnrollmentDto { StudentId = student.Id, ClassId = classSection.Id },
            currentUserId: null,
            isAdmin: true);

        Assert.True(dropResult.Success);
        Assert.Equal(204, dropResult.StatusCode);
        Assert.True(enrollResult.Success);
        Assert.Equal(201, enrollResult.StatusCode);
        Assert.Single(classSection.Enrollments);
    }

    private static EnrollmentService CreateService(IEnrollmentRepository repository)
        => new(
            repository,
            new EnrollmentQueryValidator(),
            new CreateEnrollmentValidator());

    private static Student CreateStudent(int id, string code) => new()
    {
        Id = id,
        StudentCode = code,
        FullName = $"Student {code}",
        Email = $"{code.ToLowerInvariant()}@edutrack.local",
        UserId = $"user-{id}"
    };

    private static ClassSection CreateClass(int maxCapacity) => new()
    {
        Id = 100,
        ClassCode = "CSW306-A",
        Semester = "2026-Q3",
        MaxCapacity = maxCapacity,
        CourseId = 50,
        Course = new Course
        {
            Id = 50,
            CourseCode = "CSW306",
            Name = "Backend Development",
            Credits = 4
        }
    };

    private static Enrollment CreateEnrollment(
        int id,
        Student student,
        ClassSection classSection)
    {
        var enrollment = new Enrollment
        {
            Id = id,
            StudentId = student.Id,
            Student = student,
            ClassId = classSection.Id,
            Class = classSection,
            EnrolledAt = DateTime.UtcNow
        };
        classSection.Enrollments.Add(enrollment);
        return enrollment;
    }

    private sealed class FakeEnrollmentRepository : IEnrollmentRepository
    {
        private readonly List<Student> _students;
        private readonly List<ClassSection> _classes;
        private readonly List<Enrollment> _enrollments;
        private int _nextId = 1000;

        public FakeEnrollmentRepository(
            IEnumerable<Student> students,
            IEnumerable<ClassSection> classes,
            IEnumerable<Enrollment> enrollments)
        {
            _students = students.ToList();
            _classes = classes.ToList();
            _enrollments = enrollments.ToList();
        }

        public Task<(IReadOnlyList<Enrollment> Items, int TotalCount)> GetPagedAsync(
            int? studentId,
            int? classId,
            string? semester,
            int page,
            int pageSize,
            CancellationToken cancellationToken)
        {
            var query = _enrollments.AsEnumerable();

            if (studentId.HasValue)
                query = query.Where(enrollment => enrollment.StudentId == studentId.Value);

            if (classId.HasValue)
                query = query.Where(enrollment => enrollment.ClassId == classId.Value);

            if (!string.IsNullOrWhiteSpace(semester))
            {
                query = query.Where(enrollment =>
                    string.Equals(enrollment.Class?.Semester, semester, StringComparison.Ordinal));
            }

            var filtered = query
                .OrderByDescending(enrollment => enrollment.EnrolledAt)
                .ThenBy(enrollment => enrollment.Id)
                .ToList();
            IReadOnlyList<Enrollment> items = filtered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Task.FromResult((items, filtered.Count));
        }

        public Task<Student?> GetStudentAsync(
            int studentId,
            CancellationToken cancellationToken)
            => Task.FromResult(_students.SingleOrDefault(student => student.Id == studentId));

        public Task<Student?> GetStudentByUserIdAsync(
            string userId,
            CancellationToken cancellationToken)
            => Task.FromResult(_students.SingleOrDefault(student => student.UserId == userId));

        public Task<ClassSection?> GetClassAsync(
            int classId,
            CancellationToken cancellationToken)
            => Task.FromResult(_classes.SingleOrDefault(classSection => classSection.Id == classId));

        public Task<Enrollment?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken)
            => Task.FromResult(_enrollments.SingleOrDefault(enrollment => enrollment.Id == id));

        public Task<Enrollment?> FindAsync(
            int studentId,
            int classId,
            CancellationToken cancellationToken)
            => Task.FromResult(_enrollments.SingleOrDefault(enrollment =>
                enrollment.StudentId == studentId && enrollment.ClassId == classId));

        public Task AddAsync(
            Enrollment enrollment,
            CancellationToken cancellationToken)
        {
            enrollment.Id = _nextId++;
            _enrollments.Add(enrollment);
            if (enrollment.Class is not null &&
                !enrollment.Class.Enrollments.Contains(enrollment))
            {
                enrollment.Class.Enrollments.Add(enrollment);
            }

            return Task.CompletedTask;
        }

        public void Remove(Enrollment enrollment)
        {
            _enrollments.Remove(enrollment);
            enrollment.Class?.Enrollments.Remove(enrollment);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}
