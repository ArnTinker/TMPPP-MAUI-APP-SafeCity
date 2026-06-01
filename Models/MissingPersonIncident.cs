using SafeCity.Enums;

namespace SafeCity.Models;

public class MissingPersonIncident : Incident
{
    public override string DefaultIcon => "🔍";
    public override string DefaultColor => "#2563EB";
    public override IncidentSeverity DefaultSeverity => IncidentSeverity.Critical;
    public override string[] RequiredFields => ["Title", "Description", "Address"];
}
