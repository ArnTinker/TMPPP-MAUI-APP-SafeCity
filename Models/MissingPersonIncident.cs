using SafeCity.Enums;
using SQLite;

namespace SafeCity.Models;

[Table("Incidents")]
public class MissingPersonIncident : Incident
{
    public override string DefaultIcon => "🔍";
    public override string DefaultColor => "#9A9A9A";
    public override IncidentSeverity DefaultSeverity => IncidentSeverity.Critical;
    public override string[] RequiredFields => ["Title", "Description", "Address"];
}
