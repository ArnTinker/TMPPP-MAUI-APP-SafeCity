using System.Text.Json;
using Microsoft.Maui.Controls;
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
    public virtual string DefaultColor => "#9A9A9A";
    [Ignore]
    public virtual IncidentSeverity DefaultSeverity => IncidentSeverity.Medium;
    [Ignore]
    public virtual string[] RequiredFields => ["Title", "Description"];

    /// <summary>First local file path from MediaUrlsJson, or null.</summary>
    [Ignore]
    public string? FirstMediaPath
    {
        get
        {
            try
            {
                var paths = JsonSerializer.Deserialize<List<string>>(MediaUrlsJson ?? "[]");
                return paths?.FirstOrDefault(p => !string.IsNullOrEmpty(p));
            }
            catch { return null; }
        }
    }

    /// <summary>
    /// Set by the VM after loading bytes through CachedIncidentMediaProxy.
    /// Not persisted — runtime-only display asset.
    /// </summary>
    [Ignore]
    public ImageSource? ThumbnailSource { get; set; }
}
