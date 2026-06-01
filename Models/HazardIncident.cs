using SafeCity.Enums;

namespace SafeCity.Models;

public class HazardIncident : Incident
{
    public override string DefaultIcon => "⚠️";
    public override string DefaultColor => "#FFD60A";
    public override IncidentSeverity DefaultSeverity => IncidentSeverity.Medium;
    public override string[] RequiredFields => ["Title", "Description"];
}
