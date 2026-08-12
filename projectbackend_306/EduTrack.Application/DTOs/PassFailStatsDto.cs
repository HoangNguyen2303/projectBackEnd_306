namespace EduTrack.Application.DTOs;

public class PassFailStatsDto
{
    public string Semester { get; set; } = string.Empty;
    public int TotalEnrollments { get; set; }
    public int PassedCount { get; set; }
    public int FailedCount { get; set; }
    public double PassPercentage { get; set; }
    public double FailPercentage { get; set; }
}
