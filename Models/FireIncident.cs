using SafeCity.Enums;

namespace SafeCity.Models;

public class FireIncident : Incident
{
    public override string DefaultIcon => "🔥";
    public override string DefaultColor => "#FF6B35";
    public override IncidentSeverity DefaultSeverity => IncidentSeverity.Critical;
    public override string[] RequiredFields => ["Title", "Description", "Address"];
}
