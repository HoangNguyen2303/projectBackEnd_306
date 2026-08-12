using EduTrack.Application.Interfaces;
using EduTrack.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EduTrack.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining(typeof(DependencyInjection));
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IClassService, ClassService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<ITeacherService, TeacherService>();

        return services;
    }
}
