using SafeCity.Enums;
using SQLite;

namespace SafeCity.Models;

[Table("Reports")]
public class Report
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Address { get; set; } = string.Empty;
    public IncidentType Category { get; set; }
    public IncidentSeverity Severity { get; set; }
    public string Description { get; set; } = string.Empty;
    public string MediaUrlsJson { get; set; } = "[]";
    public bool IsAnonymous { get; set; }
    public string? ReporterId { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public int? IncidentId { get; set; }
}
