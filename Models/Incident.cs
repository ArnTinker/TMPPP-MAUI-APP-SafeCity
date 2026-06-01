using SafeCity.Enums;
using SQLite;

namespace SafeCity.Models;

/// <summary>
/// Base persisted entity. Subclasses add behavioral defaults (icon, color, severity)
/// but share this single SQLite table via the repository layer.
/// </summary>
[Table("Incidents")]
public class Incident
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Address { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public IncidentType Type { get; set; }
    public IncidentSeverity Severity { get; set; }
    public IncidentStateEnum State { get; set; } = IncidentStateEnum.Reported;
    public int UpvoteCount { get; set; }
    public int ViewCount { get; set; }
    public int CommentCount { get; set; }
    public bool IsAnonymous { get; set; }
    public string? ReporterId { get; set; }
    public string MediaUrlsJson { get; set; } = "[]";

    [Ignore]
    public virtual string DefaultIcon => "📍";
    [Ignore]
    public virtual string DefaultColor => "#8E8E93";
    [Ignore]
    public virtual IncidentSeverity DefaultSeverity => IncidentSeverity.Medium;
    [Ignore]
    public virtual string[] RequiredFields => ["Title", "Description"];
}
