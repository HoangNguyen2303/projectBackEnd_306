using EduTrack.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace EduTrack.Infrastructure.Identity;

/// <summary>Login account (ASP.NET Core Identity). Linked 1-1 to a Student or Teacher.</summary>
public class AppUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsLocked { get; set; }
}
