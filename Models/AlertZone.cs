using SafeCity.Enums;
using SQLite;

namespace SafeCity.Models;

[Table("AlertZones")]
public class AlertZone
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double CenterLatitude { get; set; }
    public double CenterLongitude { get; set; }
    public double RadiusMeters { get; set; }
    public string IncidentTypesJson { get; set; } = "[]";
    public bool IsActive { get; set; } = true;
    public string? OwnerId { get; set; }
}
