using EduTrack.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EduTrack.Infrastructure.Identity;

/// <summary>Seeds sample login accounts (Admin/Teacher/Student) so the whole team can test auth without registering manually. Idempotent — safe to run on every startup. Dev-only: never call this outside Development.</summary>
public static class DbSeeder
{
    private static readonly (string Email, string Password, string FullName, UserRole Role)[] SeedUsers =
    {
        ("admin@edutrack.local", "Admin@123", "Quản trị viên", UserRole.Admin),
        ("teacher@edutrack.local", "Teacher@123", "Giáo viên Demo", UserRole.Teacher),
        ("student@edutrack.local", "Student@123", "Sinh viên Demo", UserRole.Student),
    };

    public static async Task SeedAsync(IServiceProvider services)
    {
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

        foreach (var (email, password, fullName, role) in SeedUsers)
        {
            if (await userManager.FindByEmailAsync(email) is not null)
                continue;

            var user = new AppUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                Role = role,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
                logger.LogInformation("Seeded {Role} account: {Email}", role, email);
            else
                logger.LogWarning("Failed to seed {Email}: {Errors}", email, string.Join(" ", result.Errors.Select(e => e.Description)));
        }
    }
}
