using EduTrack.Application.Common;
using EduTrack.Application.Interfaces;
using EduTrack.Application.Services;
using EduTrack.Infrastructure.Data;
using EduTrack.Infrastructure.Identity;
using EduTrack.Infrastructure.Repositories;
using EduTrack.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EduTrack.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("Default")));

        services.AddCors(options =>
        {
            options.AddPolicy("FrontendDev", policy =>
            {
                policy.WithOrigins("http://localhost:5173", "http://localhost:5174", "http://localhost:8080")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>()
            ?? throw new InvalidOperationException("Missing 'Jwt' configuration section.");
        services.AddSingleton(jwtSettings);

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<IClassRepository, ClassRepository>();
        services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<ITeacherRepository, TeacherRepository>();
        services.AddScoped<IStatisticsService, StatisticsService>();
        services.AddScoped<IGradeService, GradeService>();
        return services;
    }
}
