namespace EduTrack.Application.DTOs;

public class AuditLogDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime At { get; set; }
}
