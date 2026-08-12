using System.ComponentModel.DataAnnotations;

namespace EduTrack.Application.DTOs.Auth;

/// <summary>Self-registration always creates a Student account; Admin/Teacher accounts are provisioned by the seeder.</summary>
public class RegisterDto
{
    [Required, MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;
}
