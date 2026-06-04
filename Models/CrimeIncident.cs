using SafeCity.Enums;
using SQLite;

namespace SafeCity.Models;

[Table("Incidents")]
public class CrimeIncident : Incident
{
    public override string DefaultIcon => "🚨";
    public override string DefaultColor => "#FFD60A";
    public override IncidentSeverity DefaultSeverity => IncidentSeverity.High;
    public override string[] RequiredFields => ["Title", "Description", "Address"];
}
