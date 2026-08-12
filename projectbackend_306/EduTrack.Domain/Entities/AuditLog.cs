using EduTrack.Domain.Common;

namespace EduTrack.Domain.Entities;

/// <summary>Grade-change history, written by the service layer (no SQL trigger).</summary>
public class AuditLog : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;   // e.g. GRADE_UPDATE
    public string Entity { get; set; } = string.Empty;   // e.g. Grade#12
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime At { get; set; } = DateTime.UtcNow;
}
