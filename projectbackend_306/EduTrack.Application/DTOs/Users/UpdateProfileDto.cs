using System.ComponentModel.DataAnnotations;

namespace EduTrack.Application.DTOs.Users;

public class UpdateProfileDto
{
    [Required, MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Phone]
    public string? PhoneNumber { get; set; }
}