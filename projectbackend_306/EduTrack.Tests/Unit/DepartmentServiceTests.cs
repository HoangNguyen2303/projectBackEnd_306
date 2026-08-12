using EduTrack.Application.DTOs.Departments;
using EduTrack.Application.Interfaces;
using EduTrack.Application.Services;
using EduTrack.Application.Validators;
using EduTrack.Domain.Entities;
using Moq;

namespace EduTrack.Tests.Unit;

public sealed class DepartmentServiceTests
{
    [Fact]
    public async Task GetAsync_WhenPageIsInvalid_ReturnsBadRequest()
    {
        var repository = new Mock<IDepartmentRepository>();
        var service = CreateService(repository.Object);

        var result = await service.GetAsync(new DepartmentQueryDto { Page = 0 });

        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        repository.Verify(
            x => x.GetPagedAsync(
                It.IsAny<string?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenCodeExists_ReturnsConflict()
    {
        var repository = new Mock<IDepartmentRepository>();
        repository
            .Setup(x => x.CodeExistsAsync("IT", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var service = CreateService(repository.Object);

        var result = await service.CreateAsync(new CreateDepartmentDto
        {
            Code = "it",
            Name = "Information Technology"
        });

        Assert.False(result.Success);
        Assert.Equal(409, result.StatusCode);
        Assert.Equal("Department code already exists.", result.Message);
        repository.Verify(
            x => x.AddAsync(It.IsAny<Department>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_NormalizesAndCreatesDepartment()
    {
        var repository = new Mock<IDepartmentRepository>();
        repository
            .Setup(x => x.CodeExistsAsync("IT", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        repository
            .Setup(x => x.AddAsync(It.IsAny<Department>(), It.IsAny<CancellationToken>()))
            .Callback<Department, CancellationToken>((department, _) => department.Id = 12)
            .Returns(Task.CompletedTask);
        repository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        repository
            .Setup(x => x.GetByIdAsync(12, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Department?)null);
        var service = CreateService(repository.Object);

        var result = await service.CreateAsync(new CreateDepartmentDto
        {
            Code = "it",
            Name = "  Information Technology  "
        });

        Assert.True(result.Success);
        Assert.Equal(201, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(12, result.Data.Id);
        Assert.Equal("IT", result.Data.Code);
        Assert.Equal("Information Technology", result.Data.Name);
    }

    [Fact]
    public async Task UpdateAsync_WhenDepartmentDoesNotExist_ReturnsNotFound()
    {
        var repository = new Mock<IDepartmentRepository>();
        repository
            .Setup(x => x.GetByIdAsync(99, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Department?)null);
        var service = CreateService(repository.Object);

        var result = await service.UpdateAsync(99, new UpdateDepartmentDto
        {
            Code = "IT",
            Name = "Information Technology"
        });

        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("Department not found.", result.Message);
    }

    [Fact]
    public async Task DeleteAsync_WhenDepartmentHasRelatedData_ReturnsConflict()
    {
        var department = new Department { Id = 3, Code = "IT", Name = "Information Technology" };
        var repository = new Mock<IDepartmentRepository>();
        repository
            .Setup(x => x.GetByIdAsync(3, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(department);
        repository
            .Setup(x => x.HasRelatedDataAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var service = CreateService(repository.Object);

        var result = await service.DeleteAsync(3);

        Assert.False(result.Success);
        Assert.Equal(409, result.StatusCode);
        Assert.Equal(
            "Cannot delete a department that has students, teachers, or courses.",
            result.Message);
        repository.Verify(x => x.Remove(It.IsAny<Department>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenDepartmentIsUnused_DeletesDepartment()
    {
        var department = new Department { Id = 4, Code = "NEW", Name = "New Department" };
        var repository = new Mock<IDepartmentRepository>();
        repository
            .Setup(x => x.GetByIdAsync(4, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(department);
        repository
            .Setup(x => x.HasRelatedDataAsync(4, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        repository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var service = CreateService(repository.Object);

        var result = await service.DeleteAsync(4);

        Assert.True(result.Success);
        Assert.Equal(204, result.StatusCode);
        repository.Verify(x => x.Remove(department), Times.Once);
        repository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static DepartmentService CreateService(IDepartmentRepository repository)
        => new(
            repository,
            new DepartmentQueryValidator(),
            new CreateDepartmentValidator(),
            new UpdateDepartmentValidator());
}
