using System.ComponentModel.DataAnnotations;
using EduTrack.Domain.Enums;

namespace EduTrack.Application.DTOs.Users;

public class AdminUpdateUserDto
{
    [Required, MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    [Required]
    public UserRole Role { get; set; }
}